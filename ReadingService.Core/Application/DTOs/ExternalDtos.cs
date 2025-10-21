using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ComicDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
    }
}
