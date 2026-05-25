using KIDIO.Business.DTOs.Progress;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    /// <summary>
    /// Ghi nhận kết quả sau khi child hoàn thành 1 lesson
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<ApiResponse<ProgressResponse>>> Submit(
        [FromBody] SubmitProgressRequest request, CancellationToken ct)
    {
        var result = await _progressService.SubmitProgressAsync(
            GetCurrentUserId(), request, ct);

        return Ok(ApiResponse<ProgressResponse>.Ok(result, "Progress saved."));
    }

    /// <summary>
    /// Lấy toàn bộ lịch sử học của 1 child
    /// </summary>
    [HttpGet("child/{childId:guid}")]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProgressResponse>>>> GetByChild(
        Guid childId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _progressService.GetProgressByChildAsync(
            childId, GetCurrentUserId(), pageNumber, pageSize, ct);

        return Ok(ApiResponse<PagedResponse<ProgressResponse>>.Ok(result));
    }

    /// <summary>
    /// Lấy progress của 1 child trên 1 lesson cụ thể
    /// </summary>
    [HttpGet("child/{childId:guid}/lesson/{lessonId:guid}")]
    public async Task<ActionResult<ApiResponse<ProgressResponse?>>> GetLessonProgress(
        Guid childId, Guid lessonId, CancellationToken ct)
    {
        var result = await _progressService.GetLessonProgressAsync(
            childId, lessonId, GetCurrentUserId(), ct);

        return Ok(ApiResponse<ProgressResponse?>.Ok(result));
    }

    /// <summary>
    /// Dashboard tổng quan tiến độ của child — dùng cho Parent Dashboard
    /// </summary>
    [HttpGet("child/{childId:guid}/summary")]
    public async Task<ActionResult<ApiResponse<ChildProgressSummary>>> GetSummary(
        Guid childId, CancellationToken ct)
    {
        var result = await _progressService.GetChildSummaryAsync(
            childId, GetCurrentUserId(), ct);

        return Ok(ApiResponse<ChildProgressSummary>.Ok(result));
    }

    /// <summary>
    /// Lấy các hoạt động học gần đây của child
    /// </summary>
    [HttpGet("child/{childId:guid}/recent-activities")]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProgressResponse>>>> GetRecentActivities(
        Guid childId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _progressService.GetRecentActivitiesAsync(
            childId, GetCurrentUserId(), pageNumber, pageSize, ct);

        return Ok(ApiResponse<PagedResponse<ProgressResponse>>.Ok(result));
    }

    /// <summary>
    /// Lấy danh sách lesson đã hoàn thành của child
    /// </summary>
    [HttpGet("child/{childId:guid}/completed-lessons")]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProgressResponse>>>> GetCompletedLessons(
        Guid childId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _progressService.GetCompletedLessonsAsync(
            childId, GetCurrentUserId(), pageNumber, pageSize, ct);

        return Ok(ApiResponse<PagedResponse<ProgressResponse>>.Ok(result));
    }

    // ── Helper ──────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(value);
    }
}