using Microsoft.EntityFrameworkCore;
using ReadingService.Core.Domain.Entities;
using ReadingService.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reading.Infrastructure.Persistence.Repositories
{
    public class ComicStatsRepository : IComicStatsRepository
    {
        private readonly ReadingDbContext _context;

        public ComicStatsRepository(ReadingDbContext context)
        {
            _context = context;
        }

        public async Task<ComicStats?> GetByComicIdAsync(int comicId, CancellationToken ct = default)
        {
            return await _context.ComicStats
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ComicId == comicId, ct);
        }

        public async Task<Dictionary<int, ComicStats>> GetBatchAsync(List<int> comicIds, CancellationToken ct = default)
        {
            var stats = await _context.ComicStats
                .AsNoTracking()
                .Where(s => comicIds.Contains(s.ComicId))
                .ToListAsync(ct);

            return stats.ToDictionary(s => s.ComicId);
        }

        public async Task UpsertAsync(ComicStats stats, CancellationToken ct = default)
        {
            var existing = await _context.ComicStats.FindAsync(new object[] { stats.ComicId }, ct);

            if (existing == null)
            {
                await _context.ComicStats.AddAsync(stats, ct);
            }
            else
            {
                existing.TotalReads = stats.TotalReads;
                existing.UniqueReaders = stats.UniqueReaders;
                existing.ActiveReaders24h = stats.ActiveReaders24h;
                existing.AvgReadingTimeSeconds = stats.AvgReadingTimeSeconds;
                existing.LastUpdated = DateTime.UtcNow;
                _context.ComicStats.Update(existing);
            }

            await _context.SaveChangesAsync(ct);
        }

        public async Task RecalculateStatsAsync(int comicId, CancellationToken ct = default)
        {
            var cutoff24h = DateTime.UtcNow.AddHours(-24);

            var allReads = await _context.ReadingHistory
                .Where(h => h.ComicId == comicId)
                .ToListAsync(ct);

            if (!allReads.Any())
            {
                return;
            }

            var stats = new ComicStats
            {
                ComicId = comicId,
                TotalReads = allReads.Count,
                UniqueReaders = allReads.Select(h => h.UserId).Distinct().Count(),
                ActiveReaders24h = allReads.Where(h => h.ReadAt >= cutoff24h)
                                          .Select(h => h.UserId).Distinct().Count(),
                AvgReadingTimeSeconds = (int)allReads.Average(h => h.ReadingDurationSeconds),
                LastUpdated = DateTime.UtcNow
            };

            await UpsertAsync(stats, ct);
        }
    }
}
