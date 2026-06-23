using KIDIO.Business.DTOs.Child;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChildController : ControllerBase
{
    private readonly IChildService _childService;

    public ChildController(IChildService childService)
    {
        _childService = childService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<ChildSummaryResponse>>>> GetChildren(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _childService.GetChildrenByParentPagedAsync(GetCurrentUserId(), pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<ChildSummaryResponse>>.Ok(result));
    }

    [HttpGet("{childId:guid}")]
    public async Task<ActionResult<ApiResponse<ChildResponse>>> GetChild(
        Guid childId, CancellationToken ct)
    {
        var result = await _childService.GetChildByIdAsync(childId, GetCurrentUserId(), ct);
        return Ok(ApiResponse<ChildResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ChildResponse>>> CreateChild(
        [FromBody] CreateChildRequest request, CancellationToken ct)
    {
        var result = await _childService.CreateChildAsync(GetCurrentUserId(), request, ct);
        return CreatedAtAction(
            nameof(GetChild),
            new { childId = result.Id },
            ApiResponse<ChildResponse>.Ok(result, "Child profile created."));
    }

    [HttpPut("{childId:guid}")]
    public async Task<ActionResult<ApiResponse<ChildResponse>>> UpdateChild(
        Guid childId, [FromBody] UpdateChildRequest request, CancellationToken ct)
    {
        var result = await _childService.UpdateChildAsync(childId, GetCurrentUserId(), request, ct);
        return Ok(ApiResponse<ChildResponse>.Ok(result, "Child profile updated."));
    }

    [HttpDelete("{childId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteChild(
        Guid childId, CancellationToken ct)
    {
        await _childService.DeleteChildAsync(childId, GetCurrentUserId(), ct);
        return Ok(ApiResponse<object>.Ok(null!, "Child profile deleted."));
    }

    [HttpPatch("{childId:guid}/restore")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> RestoreChild(
        Guid childId, CancellationToken ct)
    {
        await _childService.RestoreChildAsync(childId, GetCurrentUserId(), ct);
        return Ok(ApiResponse<object>.Ok(new(), "Child profile restored."));
    }

    [HttpDelete("{childId:guid}/hard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> HardDeleteChild(
        Guid childId, CancellationToken ct)
    {
        await _childService.HardDeleteChildAsync(childId, ct);
        return Ok(ApiResponse<object>.Ok(new(), "Child profile permanently deleted."));
    }

    /// <summary>
    /// Cộng thêm Stars vào hồ sơ của Child (dùng cho Quest reward, bonus, v.v.)
    /// </summary>
    [HttpPost("{childId:guid}/add-stars")]
    public async Task<ActionResult<ApiResponse<AddStarsResponse>>> AddStars(
        Guid childId,
        [FromBody] AddStarsRequest request,
        CancellationToken ct)
    {
        var result = await _childService.AddStarsAsync(
            childId, GetCurrentUserId(), request, ct);

        return Ok(ApiResponse<AddStarsResponse>.Ok(result, "Stars added successfully."));
    }

    // ── Helper ──────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(value);
    }
}