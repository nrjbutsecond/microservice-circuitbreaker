using ComicService.Core.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.Interfaces
{
    public interface IReadingServiceClient
    {
        Task<ReadingStatsDto?> GetComicStatsAsync(int comicId, CancellationToken ct = default);
        Task<Dictionary<int, ReadingStatsDto>> GetBatchStatsAsync(List<int> comicIds, CancellationToken ct = default);
    }
}
