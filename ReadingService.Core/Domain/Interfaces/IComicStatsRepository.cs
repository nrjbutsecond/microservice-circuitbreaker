using ReadingService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Domain.Interfaces
{
    public interface IComicStatsRepository
    {
        Task<ComicStats?> GetByComicIdAsync(int comicId, CancellationToken ct = default);
        Task<Dictionary<int, ComicStats>> GetBatchAsync(List<int> comicIds, CancellationToken ct = default);
        Task UpsertAsync(ComicStats stats, CancellationToken ct = default);
        Task RecalculateStatsAsync(int comicId, CancellationToken ct = default);
    }
}
