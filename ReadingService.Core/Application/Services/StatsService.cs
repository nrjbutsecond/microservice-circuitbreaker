using Microsoft.Extensions.Logging;
using ReadingService.Core.Application.DTOs;
using ReadingService.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Application.Services
{
    public class StatsService : IStatsService
    {
        private readonly IComicStatsRepository _statsRepository;
        private readonly ILogger<StatsService> _logger;

        public StatsService(IComicStatsRepository statsRepository, ILogger<StatsService> logger)
        {
            _statsRepository = statsRepository;
            _logger = logger;
        }

        /// <summary>
        /// 🎯 ENDPOINT BỊ GỌI TỪ COMIC-SERVICE (Feature #1)
        /// </summary>
        public async Task<ReadingStatsDto?> GetComicStatsAsync(int comicId, CancellationToken ct = default)
        {
            // 🎯 DEMO: Uncomment to simulate slow response
            // await Task.Delay(5000, ct);

            _logger.LogInformation("📊 Getting stats for comic: {ComicId}", comicId);

            var stats = await _statsRepository.GetByComicIdAsync(comicId, ct);

            if (stats == null)
            {
                _logger.LogWarning("No stats found for comic: {ComicId}", comicId);
                return null;
            }

            return stats.ToDto();
        }

        /// <summary>
        /// 🎯 ENDPOINT BỊ GỌI TỪ COMIC-SERVICE (Feature #3)
        /// </summary>
        public async Task<Dictionary<int, ReadingStatsDto>> GetBatchStatsAsync(
            List<int> comicIds,
            CancellationToken ct = default)
        {
            _logger.LogInformation("📊 Getting batch stats for {Count} comics", comicIds.Count);

            var statsDict = await _statsRepository.GetBatchAsync(comicIds, ct);

            return statsDict.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDto());
        }
    }
}
