using ComicService.Core.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.Services
{
    public interface IChapterService
    {
        Task<ChapterDto> GetByNumberAsync(int comicId, int chapterNumber, CancellationToken ct = default);
        Task<List<ChapterDto>> GetByComicIdAsync(int comicId, CancellationToken ct = default);
        Task<ChapterDto> CreateAsync(int comicId, CreateChapterRequest request, CancellationToken ct = default);
    }
}
