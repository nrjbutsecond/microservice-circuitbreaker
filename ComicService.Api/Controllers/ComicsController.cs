using ComicService.Core.Application.DTOs;
using ComicService.Core.Application.Services;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComicService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComicsController : ControllerBase
    {
        private readonly IComicService _comicService;
        private readonly ILogger<ComicsController> _logger;

        public ComicsController(IComicService comicService, ILogger<ComicsController> logger)
        {
            _comicService = comicService;
            _logger = logger;
        }

        /// <summary>
        /// 🎯 FEATURE #1: Get Comic Detail with Reading Stats
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ComicDetailDto>>> GetById(int id, CancellationToken ct)
        {
            _logger.LogInformation("📖 Getting comic detail: {ComicId}", id);

            var comic = await _comicService.GetByIdAsync(id, ct);

            return Ok(ApiResponse<ComicDetailDto>.SuccessResponse(comic));
        }

        /// <summary>
        /// 🎯 FEATURE #3: Get Comics List with Trending
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<ComicListItemDto>>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            _logger.LogInformation("📚 Getting comics list: Page {Page}, Size {PageSize}", page, pageSize);

            var result = await _comicService.GetPagedAsync(page, pageSize, ct);

            return Ok(ApiResponse<PagedResult<ComicListItemDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// 🎯 FEATURE #5: Search Comics
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<ComicDto>>>> Search(
            [FromQuery] string keyword,
            CancellationToken ct)
        {
            _logger.LogInformation("🔍 Searching comics: {Keyword}", keyword);

            var results = await _comicService.SearchAsync(keyword, ct);

            return Ok(ApiResponse<List<ComicDto>>.SuccessResponse(
                results,
                $"Found {results.Count} comics"));
        }

        /// <summary>
        /// Batch get comics (called from Reading-Service)
        /// </summary>
        [HttpGet("batch")]
        public async Task<ActionResult<ApiResponse<List<ComicDto>>>> GetBatch(
            [FromQuery] List<int> ids,
            CancellationToken ct)
        {
            _logger.LogInformation("📦 Batch get comics: {Count} ids", ids.Count);

            var comics = await _comicService.GetBatchAsync(ids, ct);

            return Ok(ApiResponse<List<ComicDto>>.SuccessResponse(comics));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ComicDto>>> Create(
            [FromBody] CreateComicRequest request,
            CancellationToken ct)
        {
            var comic = await _comicService.CreateAsync(request, ct);

            return CreatedAtAction(
                nameof(GetById),
                new { id = comic.Id },
                ApiResponse<ComicDto>.SuccessResponse(comic, "Comic created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ComicDto>>> Update(
            int id,
            [FromBody] UpdateComicRequest request,
            CancellationToken ct)
        {
            var comic = await _comicService.UpdateAsync(id, request, ct);

            return Ok(ApiResponse<ComicDto>.SuccessResponse(comic, "Comic updated successfully"));
        }
    }
}