using ComicService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Domain.Interfaces
{
    public interface IChapterRepository
    {
        Task<Chapter?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Chapter?> GetByComicAndNumberAsync(int comicId, int chapterNumber, CancellationToken ct = default);
        Task<List<Chapter>> GetByComicIdAsync(int comicId, CancellationToken ct = default);
        Task<Chapter> AddAsync(Chapter chapter, CancellationToken ct = default);
        Task UpdateAsync(Chapter chapter, CancellationToken ct = default);
    }
}
