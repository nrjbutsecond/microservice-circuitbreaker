using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReadingService.Core.Application.DTOs;
using ReadingService.Core.Application.Services;

namespace ReadingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingController : ControllerBase
    {
        private readonly IReadingHistoryService _readingService;
        private readonly ILogger<ReadingController> _logger;

        public ReadingController(
            IReadingHistoryService readingService,
            ILogger<ReadingController> logger)
        {
            _readingService = readingService;
            _logger = logger;
        }

        /// <summary>
        /// 🎯 FEATURE #2: Track Reading
        /// </summary>
        [HttpPost("track")]
        public async Task<ActionResult<ApiResponse<object>>> Track(
            [FromBody] TrackReadingRequest request,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "Tracking reading: User {UserId}, Comic {ComicId}, Chapter {ChapterNumber}",
                request.UserId, request.ComicId, request.ChapterNumber);

            await _readingService.TrackReadingAsync(request, ct);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { tracked = true },
                "Reading tracked successfully"));
        }

        /// <summary>
        /// 🎯 FEATURE #4: Get User Reading History
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<ReadingHistoryDto>>>> GetHistory(
            [FromQuery] int userId,
            CancellationToken ct)
        {
            _logger.LogInformation("Getting reading history for user {UserId}", userId);

            var history = await _readingService.GetUserHistoryAsync(userId, ct);

            return Ok(ApiResponse<List<ReadingHistoryDto>>.SuccessResponse(
                history,
                $"Found {history.Count} reading records"));
        }

        /// <summary>
        /// Get User Reading Stats
        /// </summary>
        [HttpGet("stats/user/{userId}")]
        public async Task<ActionResult<ApiResponse<UserReadingStatsDto>>> GetUserStats(
            int userId,
            CancellationToken ct)
        {
            var stats = await _readingService.GetUserStatsAsync(userId, ct);
            return Ok(ApiResponse<UserReadingStatsDto>.SuccessResponse(stats));
        }
    }
}
