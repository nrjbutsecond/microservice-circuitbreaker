using ComicService.Core.Domain.Entities;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Domain.Interfaces
{
    public interface IComicRepository
    {
        Task<Comic?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<PagedResult<Comic>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
        Task<List<Comic>> SearchAsync(string keyword, CancellationToken ct = default);
        Task<List<Comic>> GetByIdsAsync(List<int> ids, CancellationToken ct = default);
        Task<Comic> AddAsync(Comic comic, CancellationToken ct = default);
        Task UpdateAsync(Comic comic, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
