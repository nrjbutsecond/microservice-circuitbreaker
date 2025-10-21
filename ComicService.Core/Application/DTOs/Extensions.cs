using ComicService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.DTOs
{
    public static class Extensions
    {
        public static ComicDto ToDto(this Comic comic)
        {
            return new ComicDto
            {
                Id = comic.Id,
                Title = comic.Title,
                Author = comic.Author,
                Description = comic.Description,
                CoverImageUrl = comic.CoverImageUrl,
                TotalChapters = comic.TotalChapters,
                Status = comic.Status.ToString(),
                CreatedAt = comic.CreatedAt,
                UpdatedAt = comic.UpdatedAt
            };
        }

        public static ChapterDto ToDto(this Chapter chapter)
        {
            return new ChapterDto
            {
                Id = chapter.Id,
                ComicId = chapter.ComicId,
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title,
                Content = chapter.Content,
                ViewCount = chapter.ViewCount,
                CreatedAt = chapter.CreatedAt
            };
        }
    }
}