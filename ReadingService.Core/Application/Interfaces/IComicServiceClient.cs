using ReadingService.Core.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Application.Interfaces
{
    public interface IComicServiceClient
    {
        Task<ComicDto?> GetComicAsync(int comicId, CancellationToken ct = default);
        Task<Dictionary<int, ComicDto>> GetBatchComicsAsync(List<int> comicIds, CancellationToken ct = default);
    }
}
