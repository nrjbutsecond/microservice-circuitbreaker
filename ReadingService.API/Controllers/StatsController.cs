using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReadingService.Core.Application.DTOs;
using ReadingService.Core.Application.Services;

namespace ReadingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;
        private readonly ILogger<StatsController> _logger;

        public StatsController(IStatsService statsService, ILogger<StatsController> logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        /// <summary>
        /// 🔴 ENDPOINT BỊ GỌI TỪ COMIC-SERVICE (Feature #1)
        /// </summary>
        [HttpGet("comic/{comicId}")]
        public async Task<ActionResult<ReadingStatsDto>> GetComicStats(
            int comicId,
            CancellationToken ct)
        {
            _logger.LogInformation("📊 Stats request for comic: {ComicId}", comicId);

            // 🎯 DEMO: Uncomment to simulate slow response (trigger timeout)
            // await Task.Delay(5000, ct);

            // 🎯 DEMO: Uncomment to simulate failure
            // return StatusCode(500, "Service temporarily unavailable");

            var stats = await _statsService.GetComicStatsAsync(comicId, ct);

            if (stats == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"No stats found for comic {comicId}"));
            }

            return Ok(stats);
        }

        /// <summary>
        /// 🔴 ENDPOINT BỊ GỌI TỪ COMIC-SERVICE (Feature #3)
        /// </summary>
        [HttpGet("comics/batch")]
        public async Task<ActionResult<Dictionary<Guid, ReadingStatsDto>>> GetBatchStats(
            [FromQuery] List<int> ids,
            CancellationToken ct)
        {
            _logger.LogInformation("📊 Batch stats request for {Count} comics", ids.Count);

            var stats = await _statsService.GetBatchStatsAsync(ids, ct);

            return Ok(stats);
        }
    }

}
