using ComicService.Core.Application.DTOs;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.Services
{
    public interface IComicService
    {
        Task<ComicDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
        Task<PagedResult<ComicListItemDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
        Task<List<ComicDto>> SearchAsync(string keyword, CancellationToken ct = default);
        Task<List<ComicDto>> GetBatchAsync(List<int> ids, CancellationToken ct = default);
        Task<ComicDto> CreateAsync(CreateComicRequest request, CancellationToken ct = default);
        Task<ComicDto> UpdateAsync(int id, UpdateComicRequest request, CancellationToken ct = default);
    }
}
