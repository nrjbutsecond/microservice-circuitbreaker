using ComicService.Core.Domain.Entities;
using ComicService.Core.Domain.Interfaces;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Infrastructure.Persistence.Repositories
{
    public class ComicRepository : IComicRepository
    {
        private readonly ComicDbContext _context;

        public ComicRepository(ComicDbContext context)
        {
            _context = context;
        }

        public async Task<Comic?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Comics
                .AsNoTracking()
                .Include(c => c.Chapters)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<PagedResult<Comic>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Comics.AsNoTracking();

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(c => c.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Comic>(items, totalCount, page, pageSize);
        }

        public async Task<List<Comic>> SearchAsync(string keyword, CancellationToken ct = default)
        {
            return await _context.Comics
                .AsNoTracking()
                .Where(c => c.Title.Contains(keyword) ||
                           (c.Author != null && c.Author.Contains(keyword)))
                .OrderByDescending(c => c.UpdatedAt)
                .Take(20)
                .ToListAsync(ct);
        }

        public async Task<List<Comic>> GetByIdsAsync(List<int> ids, CancellationToken ct = default)
        {
            return await _context.Comics
                .AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .ToListAsync(ct);
        }

        public async Task<Comic> AddAsync(Comic comic, CancellationToken ct = default)
        {
            await _context.Comics.AddAsync(comic, ct);
            await _context.SaveChangesAsync(ct);
            return comic;
        }

        public async Task UpdateAsync(Comic comic, CancellationToken ct = default)
        {
            _context.Comics.Update(comic);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var comic = await _context.Comics.FindAsync(new object[] { id }, ct);
            if (comic != null)
            {
                _context.Comics.Remove(comic);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}