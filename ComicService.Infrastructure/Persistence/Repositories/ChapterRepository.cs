using ComicService.Core.Domain.Entities;
using ComicService.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Infrastructure.Persistence.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly ComicDbContext _context;

        public ChapterRepository(ComicDbContext context)
        {
            _context = context;
        }

        public async Task<Chapter?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Chapters
                .AsNoTracking()
                .Include(ch => ch.Comic)
                .FirstOrDefaultAsync(ch => ch.Id == id, ct);
        }

        public async Task<Chapter?> GetByComicAndNumberAsync(int comicId, int chapterNumber, CancellationToken ct = default)
        {
            return await _context.Chapters
                .AsNoTracking()
                .Include(ch => ch.Comic)
                .FirstOrDefaultAsync(ch => ch.ComicId == comicId && ch.ChapterNumber == chapterNumber, ct);
        }

        public async Task<List<Chapter>> GetByComicIdAsync(int comicId, CancellationToken ct = default)
        {
            return await _context.Chapters
                .AsNoTracking()
                .Where(ch => ch.ComicId == comicId)
                .OrderBy(ch => ch.ChapterNumber)
                .ToListAsync(ct);
        }

        public async Task<Chapter> AddAsync(Chapter chapter, CancellationToken ct = default)
        {
            await _context.Chapters.AddAsync(chapter, ct);
            await _context.SaveChangesAsync(ct);

            // Update comic's total chapters
            var comic = await _context.Comics.FindAsync(new object[] { chapter.ComicId }, ct);
            if (comic != null)
            {
                comic.TotalChapters = await _context.Chapters.CountAsync(ch => ch.ComicId == chapter.ComicId, ct);
                comic.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }

            return chapter;
        }

        public async Task UpdateAsync(Chapter chapter, CancellationToken ct = default)
        {
            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync(ct);
        }
    }
}
