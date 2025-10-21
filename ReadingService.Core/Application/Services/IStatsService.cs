using ReadingService.Core.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Application.Services
{
    public interface IStatsService
    {
        Task<ReadingStatsDto?> GetComicStatsAsync(int comicId, CancellationToken ct = default);
        Task<Dictionary<int, ReadingStatsDto>> GetBatchStatsAsync(List<int> comicIds, CancellationToken ct = default);
    }
}
