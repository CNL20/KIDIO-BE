using KIDIO.Business.DTOs.Lesson;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    // Public — child/parent đều xem được
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<TopicSummaryResponse>>>> GetTopics(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _topicService.GetTopicsPagedAsync(pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<TopicSummaryResponse>>.Ok(result));
    }

    [HttpGet("{topicId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TopicResponse>>> GetTopic(
        Guid topicId, CancellationToken ct)
    {
        var result = await _topicService.GetTopicByIdAsync(topicId, ct);
        return Ok(ApiResponse<TopicResponse>.Ok(result));
    }

    // Admin only
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TopicResponse>>> CreateTopic(
        [FromBody] CreateTopicRequest request, CancellationToken ct)
    {
        var result = await _topicService.CreateTopicAsync(request, ct);
        return CreatedAtAction(nameof(GetTopic),
            new { topicId = result.Id },
            ApiResponse<TopicResponse>.Ok(result, "Topic created."));
    }

    [HttpPut("{topicId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TopicResponse>>> UpdateTopic(
        Guid topicId, [FromBody] UpdateTopicRequest request, CancellationToken ct)
    {
        var result = await _topicService.UpdateTopicAsync(topicId, request, ct);
        return Ok(ApiResponse<TopicResponse>.Ok(result, "Topic updated."));
    }

    [HttpDelete("{topicId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTopic(
        Guid topicId, CancellationToken ct)
    {
        await _topicService.DeleteTopicAsync(topicId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Topic deleted."));
    }

    [HttpPatch("{topicId:guid}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> RestoreTopic(
        Guid topicId, CancellationToken ct)
    {
        await _topicService.RestoreTopicAsync(topicId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Topic restored."));
    }

    [HttpDelete("{topicId:guid}/hard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> HardDeleteTopic(
        Guid topicId, CancellationToken ct)
    {
        await _topicService.HardDeleteTopicAsync(topicId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Topic permanently deleted."));
    }
}