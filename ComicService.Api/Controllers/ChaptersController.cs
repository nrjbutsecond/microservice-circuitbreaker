using ComicService.Core.Application.DTOs;
using ComicService.Core.Application.Services;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComicService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChaptersController : ControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly ILogger<ChaptersController> _logger;

        public ChaptersController(IChapterService chapterService, ILogger<ChaptersController> logger)
        {
            _chapterService = chapterService;
            _logger = logger;
        }

        /// <summary>
        /// 🎯 FEATURE #2: Get Chapter to Read
        /// </summary>
        [HttpGet("{chapterNumber}")]
        public async Task<ActionResult<ApiResponse<ChapterDto>>> GetByNumber(
            int comicId,
            int chapterNumber,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "📄 Getting chapter: Comic {ComicId}, Chapter {ChapterNumber}",
                comicId, chapterNumber);

            var chapter = await _chapterService.GetByNumberAsync(comicId, chapterNumber, ct);

            return Ok(ApiResponse<ChapterDto>.SuccessResponse(chapter));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ChapterDto>>>> GetAll(
            int comicId,
            CancellationToken ct)
        {
            var chapters = await _chapterService.GetByComicIdAsync(comicId, ct);

            return Ok(ApiResponse<List<ChapterDto>>.SuccessResponse(chapters));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ChapterDto>>> Create(
            int comicId,
            [FromBody] CreateChapterRequest request,
            CancellationToken ct)
        {
            var chapter = await _chapterService.CreateAsync(comicId, request, ct);

            return CreatedAtAction(
                nameof(GetByNumber),
                new { comicId, chapterNumber = chapter.ChapterNumber },
                ApiResponse<ChapterDto>.SuccessResponse(chapter, "Chapter created successfully"));
        }
    }
}
