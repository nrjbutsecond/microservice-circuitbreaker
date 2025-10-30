using ComicService.Core.Application.DTOs;
using ComicService.Core.Application.Interfaces;
using ComicService.Core.Domain.Entities;
using ComicService.Core.Domain.Interfaces;
using Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Shared.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.Services
{
    public class ComicService : IComicService
    {
        private readonly IComicRepository _comicRepository;
        private readonly IReadingServiceClient _readingClient;
        // private readonly IMemoryCache _cache;
        private readonly ILogger<ComicService> _logger;

        public ComicService(
            IComicRepository comicRepository,
            IReadingServiceClient readingClient,
            IMemoryCache cache,
            ILogger<ComicService> logger)
        {
            _comicRepository = comicRepository;
            _readingClient = readingClient;
            // _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 🎯 FEATURE #1: Xem Chi Tiết Truyện với Reading Stats
        /// 🔴 CIRCUIT BREAKER POINT: Reading-Service
        /// </summary>
        public async Task<ComicDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var comic = await _comicRepository.GetByIdAsync(id, ct);
            if (comic == null)
            {
                throw new NotFoundException(nameof(Comic), id);
            }

            var dto = new ComicDetailDto
            {
                Id = comic.Id,
                Title = comic.Title,
                Author = comic.Author,
                Description = comic.Description,
                CoverImageUrl = comic.CoverImageUrl,
                TotalChapters = comic.TotalChapters,
                Status = comic.Status.ToString(),
                CreatedAt = comic.CreatedAt,
                UpdatedAt = comic.UpdatedAt
            };

            // 🔴 CIRCUIT BREAKER: Get reading stats from Reading-Service
            try
            {
                _logger.LogInformation("🔹 Calling Reading-Service for comic stats: {ComicId}", id);

                var stats = await _readingClient.GetComicStatsAsync(id, ct);

                if (stats != null)
                {
                    dto.TotalReads = stats.TotalReads;
                    dto.UniqueReaders = stats.UniqueReaders;
                    dto.ActiveReaders24h = stats.ActiveReaders24h;
                    dto.IsTrending = stats.ActiveReaders24h > 50;

                    // // Cache stats for fallback
                    // _cache.Set($"stats:{id}", stats, TimeSpan.FromSeconds(10));

                    _logger.LogInformation("✅ Stats fetched successfully for comic {ComicId}", id);
                }
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(ex,
                    "🔴 CIRCUIT BREAKER OPEN: Reading-Service unavailable for comic {ComicId}. Using fallback.",
                    id);

                // Fallback: Use cached stats
                // var cachedStats = _cache.Get<ReadingStatsDto>($"stats:{id}");
                // if (cachedStats != null)
                // {
                //     dto.TotalReads = cachedStats.TotalReads;
                //     dto.UniqueReaders = cachedStats.UniqueReaders;
                //     dto.ActiveReaders24h = cachedStats.ActiveReaders24h;
                //     dto.IsTrending = cachedStats.ActiveReaders24h > 50;
                //     _logger.LogInformation("✅ Using cached stats for comic {ComicId}", id);
                // }
                // else
                // {
                //     _logger.LogInformation("⚠️ No cached stats available for comic {ComicId}", id);
                // }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "⚠️ Failed to fetch stats for comic {ComicId}. Continuing without stats.",
                    id);
            }

            return dto;
        }

        /// <summary>
        /// 🎯 FEATURE #3: Danh Sách Truyện với Trending Stats
        /// 🔴 CIRCUIT BREAKER POINT: Batch call to Reading-Service
        /// </summary>
        public async Task<PagedResult<ComicListItemDto>> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var comics = await _comicRepository.GetPagedAsync(page, pageSize, ct);
            var comicIds = comics.Items.Select(c => c.Id).ToList();

            // 🔴 CIRCUIT BREAKER: Batch get stats
            Dictionary<int, ReadingStatsDto> statsDict = new();

            try
            {
                _logger.LogInformation("🔹 Calling Reading-Service for batch stats: {Count} comics", comicIds.Count);

                statsDict = await _readingClient.GetBatchStatsAsync(comicIds, ct);

                // Cache all stats
                // foreach (var kvp in statsDict)
                // {
                //     _cache.Set($"stats:{kvp.Key}", kvp.Value, TimeSpan.FromMinutes(5));
                // }

                _logger.LogInformation("✅ Batch stats fetched: {Count} results", statsDict.Count);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(ex,
                    "🔴 CIRCUIT BREAKER OPEN: Reading-Service unavailable. Using cached stats.");

                // Fallback: Try to get cached stats
                // foreach (var comicId in comicIds)
                // {
                //     var cached = _cache.Get<ReadingStatsDto>($"stats:{comicId}");
                //     if (cached != null)
                //     {
                //         statsDict[comicId] = cached;
                //     }
                // }

                _logger.LogInformation("✅ Using cached stats: {Count}/{Total}",
                    statsDict.Count, comicIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to fetch batch stats. Continuing without stats.");
            }

            var dtos = comics.Items.Select(comic =>
            {
                var stats = statsDict.GetValueOrDefault(comic.Id);
                return new ComicListItemDto
                {
                    Id = comic.Id,
                    Title = comic.Title,
                    Author = comic.Author,
                    Description = comic.Description,
                    CoverImageUrl = comic.CoverImageUrl,
                    TotalChapters = comic.TotalChapters,
                    Status = comic.Status.ToString(),
                    CreatedAt = comic.CreatedAt,
                    UpdatedAt = comic.UpdatedAt,
                    TotalReads = stats?.TotalReads ?? 0,
                    IsTrending = (stats?.ActiveReaders24h ?? 0) > 50
                };
            }).ToList();

            return new PagedResult<ComicListItemDto>(dtos, comics.TotalCount, page, pageSize);
        }

        /// <summary>
        /// 🎯 FEATURE #5: Tìm Kiếm Truyện
        /// 🔴 CIRCUIT BREAKER POINT: Optional popularity boost
        /// </summary>
        public async Task<List<ComicDto>> SearchAsync(string keyword, CancellationToken ct = default)
        {
            var comics = await _comicRepository.SearchAsync(keyword, ct);

            // 🔴 CIRCUIT BREAKER: Optional popularity boost (non-critical)
            try
            {
                var comicIds = comics.Select(c => c.Id).ToList();
                var statsDict = await _readingClient.GetBatchStatsAsync(comicIds, ct);

                // Sort by popularity
                comics = comics
                    .OrderByDescending(c => statsDict.GetValueOrDefault(c.Id)?.TotalReads ?? 0)
                    .ToList();

                _logger.LogInformation("✅ Search results sorted by popularity");
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(ex,
                    "🔴 CIRCUIT BREAKER OPEN: Skipping popularity boost for search");
                // Keep original order (by relevance)
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to boost by popularity. Using default order.");
            }

            return comics.Select(c => c.ToDto()).ToList();
        }

        public async Task<List<ComicDto>> GetBatchAsync(List<int> ids, CancellationToken ct = default)
        {
            var comics = await _comicRepository.GetByIdsAsync(ids, ct);
            return comics.Select(c => c.ToDto()).ToList();
        }

        public async Task<ComicDto> CreateAsync(CreateComicRequest request, CancellationToken ct = default)
        {
            var comic = new Comic
            {
                Title = request.Title,
                Author = request.Author,
                Description = request.Description,
                CoverImageUrl = request.CoverImageUrl,
                Status = ComicStatus.Ongoing,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _comicRepository.AddAsync(comic, ct);

            _logger.LogInformation("✅ Comic created: {ComicId} - {Title}", comic.Id, comic.Title);

            return comic.ToDto();
        }

        public async Task<ComicDto> UpdateAsync(int id, UpdateComicRequest request, CancellationToken ct = default)
        {
            var comic = await _comicRepository.GetByIdAsync(id, ct);
            if (comic == null)
            {
                throw new NotFoundException(nameof(Comic), id);
            }

            if (request.Title != null) comic.Title = request.Title;
            if (request.Author != null) comic.Author = request.Author;
            if (request.Description != null) comic.Description = request.Description;
            if (request.CoverImageUrl != null) comic.CoverImageUrl = request.CoverImageUrl;
            if (request.Status != null && Enum.TryParse<ComicStatus>(request.Status, out var status))
            {
                comic.Status = status;
            }

            comic.UpdatedAt = DateTime.UtcNow;

            await _comicRepository.UpdateAsync(comic, ct);

            _logger.LogInformation("✅ Comic updated: {ComicId}", id);

            return comic.ToDto();
        }
    }
}