using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Application.DTOs;

namespace UserService.Core.Application.Service
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    }
}
