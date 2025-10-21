using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Application.DTOs;

namespace UserService.Core.Application.Service
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default);
        Task<bool> ValidateUserAsync(int userId, CancellationToken ct = default);
        Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
        Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    }
}
