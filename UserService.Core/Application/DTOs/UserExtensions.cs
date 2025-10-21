using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Domain.Entities;

namespace UserService.Core.Application.DTOs
{
    public static class UserExtensions
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}