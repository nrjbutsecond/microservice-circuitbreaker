using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Domain.Entities;
using UserService.Core.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username, ct);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == id && u.IsActive, ct);
        }

        public async Task<User> AddAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
            return user;
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email, ct);
        }

        public async Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username, ct);
        }
    }
}