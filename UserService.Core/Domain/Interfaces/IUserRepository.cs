using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Domain.Entities;

namespace UserService.Core.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task<User> AddAsync(User user, CancellationToken ct = default);
        Task UpdateAsync(User user, CancellationToken ct = default);
        Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);
        Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default);
    }
}
