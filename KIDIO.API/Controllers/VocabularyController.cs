using KIDIO.Business.DTOs.Vocabulary;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyService _vocabService;

    public VocabularyController(IVocabularyService vocabService)
    {
        _vocabService = vocabService;
    }

    [HttpGet("paged")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<VocabularyResponse>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? lessonId = null,
        CancellationToken ct = default)
    {
        var result = await _vocabService.GetPagedAsync(page, pageSize, lessonId, ct);
        return Ok(ApiResponse<PagedResponse<VocabularyResponse>>.Ok(result));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<VocabularyResponse>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _vocabService.GetAllPagedAsync(pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<VocabularyResponse>>.Ok(result));
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<VocabularyResponse>>>> Search(
        [FromQuery] string keyword,
        [FromQuery] Guid? lessonId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _vocabService.SearchPagedAsync(keyword, lessonId, pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<VocabularyResponse>>.Ok(result));
    }

    [HttpGet("lesson/{lessonId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<VocabularyResponse>>>> GetByLesson(
        Guid lessonId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _vocabService.GetByLessonPagedAsync(lessonId, pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<VocabularyResponse>>.Ok(result));
    }

    [HttpGet("{vocabId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<VocabularyResponse>>> GetById(
        Guid vocabId, CancellationToken ct)
    {
        var result = await _vocabService.GetByIdAsync(vocabId, ct);
        return Ok(ApiResponse<VocabularyResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<VocabularyResponse>>> Create(
        [FromBody] CreateVocabularyRequest request, CancellationToken ct)
    {
        var result = await _vocabService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById),
            new { vocabId = result.Id },
            ApiResponse<VocabularyResponse>.Ok(result, "Vocabulary created."));
    }

    [HttpPut("{vocabId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<VocabularyResponse>>> Update(
        Guid vocabId, [FromBody] UpdateVocabularyRequest request, CancellationToken ct)
    {
        var result = await _vocabService.UpdateAsync(vocabId, request, ct);
        return Ok(ApiResponse<VocabularyResponse>.Ok(result, "Vocabulary updated."));
    }

    [HttpDelete("{vocabId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid vocabId, CancellationToken ct)
    {
        await _vocabService.DeleteAsync(vocabId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Vocabulary deleted."));
    }

    [HttpPatch("{vocabId:guid}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Restore(
        Guid vocabId, CancellationToken ct)
    {
        await _vocabService.RestoreAsync(vocabId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Vocabulary restored."));
    }

    [HttpDelete("{vocabId:guid}/hard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> HardDelete(
        Guid vocabId, CancellationToken ct)
    {
        await _vocabService.HardDeleteAsync(vocabId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Vocabulary permanently deleted."));
    }
}