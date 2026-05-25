using KIDIO.Business.DTOs.Lesson;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    // Public
    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<LessonSummaryResponse>>>> GetAllLessons(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var includeUnpublished = User.IsInRole("Admin");
        var result = await _lessonService.GetAllLessonsPagedAsync(includeUnpublished, pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<LessonSummaryResponse>>.Ok(result));
    }

    [HttpGet("topic/{topicId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<LessonSummaryResponse>>>> GetLessonsByTopic(
        Guid topicId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var includeUnpublished = User.IsInRole("Admin");
        var result = await _lessonService.GetLessonsByTopicPagedAsync(topicId, includeUnpublished, pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<LessonSummaryResponse>>.Ok(result));
    }

    [HttpGet("{lessonId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LessonResponse>>> GetLesson(
        Guid lessonId, CancellationToken ct)
    {
        var includeUnpublished = User.IsInRole("Admin");
        var result = await _lessonService.GetLessonByIdAsync(lessonId, includeUnpublished, ct);
        return Ok(ApiResponse<LessonResponse>.Ok(result));
    }

    // Admin only
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<LessonResponse>>> CreateLesson(
        [FromBody] CreateLessonRequest request, CancellationToken ct)
    {
        var result = await _lessonService.CreateLessonAsync(request, ct);
        return CreatedAtAction(nameof(GetLesson),
            new { lessonId = result.Id },
            ApiResponse<LessonResponse>.Ok(result, "Lesson created."));
    }

    [HttpPut("{lessonId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<LessonResponse>>> UpdateLesson(
        Guid lessonId, [FromBody] UpdateLessonRequest request, CancellationToken ct)
    {
        var result = await _lessonService.UpdateLessonAsync(lessonId, request, ct);
        return Ok(ApiResponse<LessonResponse>.Ok(result, "Lesson updated."));
    }

    [HttpDelete("{lessonId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteLesson(
        Guid lessonId, CancellationToken ct)
    {
        await _lessonService.DeleteLessonAsync(lessonId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Lesson deleted."));
    }

    [HttpPatch("{lessonId:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Publish(
        Guid lessonId, CancellationToken ct)
    {
        await _lessonService.PublishLessonAsync(lessonId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Lesson published."));
    }

    [HttpPatch("{lessonId:guid}/unpublish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Unpublish(
        Guid lessonId, CancellationToken ct)
    {
        await _lessonService.UnpublishLessonAsync(lessonId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Lesson unpublished."));
    }

    [HttpPatch("{lessonId:guid}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Restore(
        Guid lessonId, CancellationToken ct)
    {
        await _lessonService.RestoreLessonAsync(lessonId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Lesson restored."));
    }

    [HttpDelete("{lessonId:guid}/hard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> HardDelete(
        Guid lessonId, CancellationToken ct)
    {
        await _lessonService.HardDeleteLessonAsync(lessonId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Lesson permanently deleted."));
    }
}