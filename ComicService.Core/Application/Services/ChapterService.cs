using ComicService.Core.Application.DTOs;
using ComicService.Core.Domain.Entities;
using ComicService.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.Services
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IComicRepository _comicRepository;
        private readonly ILogger<ChapterService> _logger;

        public ChapterService(
            IChapterRepository chapterRepository,
            IComicRepository comicRepository,
            ILogger<ChapterService> logger)
        {
            _chapterRepository = chapterRepository;
            _comicRepository = comicRepository;
            _logger = logger;
        }

        public async Task<ChapterDto> GetByNumberAsync(int comicId, int chapterNumber, CancellationToken ct = default)
        {
            var chapter = await _chapterRepository.GetByComicAndNumberAsync(comicId, chapterNumber, ct);

            if (chapter == null)
            {
                throw new NotFoundException($"Chapter {chapterNumber} not found for comic {comicId}");
            }

            // Increment view count
            chapter.ViewCount++;
            await _chapterRepository.UpdateAsync(chapter, ct);

            return chapter.ToDto();
        }

        public async Task<List<ChapterDto>> GetByComicIdAsync(int comicId, CancellationToken ct = default)
        {
            var chapters = await _chapterRepository.GetByComicIdAsync(comicId, ct);
            return chapters.Select(ch => ch.ToDto()).ToList();
        }

        public async Task<ChapterDto> CreateAsync(int comicId, CreateChapterRequest request, CancellationToken ct = default)
        {
            var comic = await _comicRepository.GetByIdAsync(comicId, ct);
            if (comic == null)
            {
                throw new NotFoundException(nameof(Comic), comicId);
            }

            var chapter = new Chapter
            {
                ComicId = comicId,
                ChapterNumber = request.ChapterNumber,
                Title = request.Title,
                Content = request.Content,
                ViewCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _chapterRepository.AddAsync(chapter, ct);

            _logger.LogInformation(
                "✅ Chapter created: Comic {ComicId}, Chapter {ChapterNumber}",
                comicId, request.ChapterNumber);

            return chapter.ToDto();
        }
    }
}