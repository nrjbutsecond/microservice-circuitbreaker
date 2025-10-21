using ReadingService.Core.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Application.Interfaces
{
    public interface IUserServiceClient
    {
        Task<bool> ValidateUserAsync(int userId, CancellationToken ct = default);
        Task<UserDto?> GetUserAsync(int userId, CancellationToken ct = default);
    }
}
