using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Domain.Entities;

namespace UserService.Core.Application.Service
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        int? ValidateToken(string token);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}
