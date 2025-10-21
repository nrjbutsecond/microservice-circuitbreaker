using ReadingService.Core.Application.DTOs;
using ReadingService.Core.Application.Interfaces;
using ReadingService.Core.Domain.Entities;
using ReadingService.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace ReadingService.Core.Application.Services
{
    public class ReadingHistoryService : IReadingHistoryService
    {
        private readonly IReadingHistoryRepository _readingRepository;
        private readonly IComicStatsRepository _statsRepository;
        private readonly IUserServiceClient _userClient;
        private readonly IComicServiceClient _comicClient;
        private readonly ILogger<ReadingHistoryService> _logger;

        public ReadingHistoryService(
            IReadingHistoryRepository readingRepository,
            IComicStatsRepository statsRepository,
            IUserServiceClient userClient,
            IComicServiceClient comicClient,
            ILogger<ReadingHistoryService> logger)
        {
            _readingRepository = readingRepository;
            _statsRepository = statsRepository;
            _userClient = userClient;
            _comicClient = comicClient;
            _logger = logger;
        }

        /// <summary>
        /// 🎯 FEATURE #2: Đọc Truyện + Track Reading History
        /// </summary>
        public async Task TrackReadingAsync(TrackReadingRequest request, CancellationToken ct = default)
        {
            // 🔴 CIRCUIT BREAKER POINT #1: Validate User
            bool userValid = false;
            try
            {
                userValid = await _userClient.ValidateUserAsync(request.UserId, ct);
                _logger.LogInformation("User validation successful: {UserId}", request.UserId);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(ex,
                    "🔴 CIRCUIT BREAKER OPEN: User-Service unavailable. Skipping validation for user {UserId}",
                    request.UserId);

                // Fallback: Lưu vào queue để verify sau (trong production sẽ dùng message queue)
                // Tạm thời assume user is valid
                userValid = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "⚠️ User validation failed: {UserId}. Proceeding with caution.",
                    request.UserId);
                userValid = true; // Assume valid for demo
            }

            if (!userValid)
            {
                throw new UnauthorizedAccessException("Invalid user");
            }

            // Lưu reading history
            var history = new ReadingHistory
            {
                UserId = request.UserId,
                ComicId = request.ComicId,
                ChapterId = request.ChapterId,
                ChapterNumber = request.ChapterNumber,
                ReadAt = DateTime.UtcNow,
                ReadingDurationSeconds = request.ReadingDurationSeconds,
                Completed = request.Completed
            };

            await _readingRepository.AddAsync(history, ct);

            // Recalculate stats asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    await _statsRepository.RecalculateStatsAsync(request.ComicId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to recalculate stats for comic {ComicId}", request.ComicId);
                }
            }, ct);

            _logger.LogInformation(
                "✅ Reading tracked: User {UserId} read Comic {ComicId}, Chapter {ChapterNumber}",
                request.UserId, request.ComicId, request.ChapterNumber);
        }

        /// <summary>
        /// 🎯 FEATURE #4: Lịch Sử Đọc của User
        /// </summary>
        public async Task<List<ReadingHistoryDto>> GetUserHistoryAsync(int userId, CancellationToken ct = default)
        {
            var history = await _readingRepository.GetByUserIdAsync(userId, ct);

            if (!history.Any())
            {
                return new List<ReadingHistoryDto>();
            }

            // 🔴 CIRCUIT BREAKER POINT #2: Get User Info
            UserDto? user = null;
            try
            {
                user = await _userClient.GetUserAsync(userId, ct);
                _logger.LogInformation("Successfully fetched user info: {UserId}", userId);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(ex,
                    "🔴 CIRCUIT BREAKER OPEN: User-Service unavailable. Using fallback user info.");
                user = new UserDto { Id = userId, Username = "Unknown User" };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch user info. Using fallback.");
                user = new UserDto { Id = userId, Username = "Unknown User" };
            }

            // 🔴 CIRCUIT BREAKER POINT #3: Get Comic Details (batch)
            var comicIds = history.Select(h => h.ComicId).Distinct().ToList();
            Dictionary<int, ComicDto> comicsDict;

            try
            {
                comicsDict = await _comicClient.GetBatchComicsAsync(comicIds, ct);
                _logger.LogInformation("Successfully fetched {Count} comics", comicsDict.Count);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(ex,
                    "🔴 CIRCUIT BREAKER OPEN: Comic-Service unavailable. Using fallback comic data.");
                comicsDict = comicIds.ToDictionary(
                    id => id,
                    id => new ComicDto { Id = id, Title = "Unknown Comic" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch comic details. Using fallback.");
                comicsDict = comicIds.ToDictionary(
                    id => id,
                    id => new ComicDto { Id = id, Title = "Unknown Comic" });
            }

            return history.Select(h =>
            {
                var dto = h.ToDto();
                dto.UserName = user?.Username;
                dto.ComicTitle = comicsDict.GetValueOrDefault(h.ComicId)?.Title;
                return dto;
            }).ToList();
        }

        public async Task<UserReadingStatsDto> GetUserStatsAsync(int userId, CancellationToken ct = default)
        {
            var history = await _readingRepository.GetByUserIdAsync(userId, ct);

            if (!history.Any())
            {
                return new UserReadingStatsDto();
            }

            return new UserReadingStatsDto
            {
                TotalComicsRead = history.Select(h => h.ComicId).Distinct().Count(),
                TotalChaptersRead = history.Count,
                TotalReadingTimeHours = history.Sum(h => h.ReadingDurationSeconds) / 3600,
                LastReadAt = history.Max(h => h.ReadAt)
            };
        }
    }
}