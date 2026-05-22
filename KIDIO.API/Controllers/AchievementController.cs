using KIDIO.Business.DTOs.Achievement;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    /// <summary>
    /// Lấy tất cả huy hiệu của child — dùng cho trang profile
    /// </summary>
    [HttpGet("child/{childId:guid}")]
    public async Task<ActionResult<ApiResponse<List<AchievementResponse>>>> GetByChild(
        Guid childId, CancellationToken ct)
    {
        var result = await _achievementService.GetByChildAsync(
            childId, GetCurrentUserId(), ct);

        return Ok(ApiResponse<List<AchievementResponse>>.Ok(result));
    }

    /// <summary>
    /// [Admin] Lấy tất cả định nghĩa huy hiệu (bao gồm cả inactive)
    /// </summary>
    [HttpGet("definitions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<AchievementDefinitionResponse>>>> GetAllDefinitions(CancellationToken ct)
    {
        var result = await _achievementService.GetAllDefinitionsAsync(ct);
        return Ok(ApiResponse<List<AchievementDefinitionResponse>>.Ok(result));
    }

    /// <summary>
    /// [Admin] Lấy chi tiết một định nghĩa huy hiệu
    /// </summary>
    [HttpGet("definitions/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AchievementDefinitionResponse>>> GetDefinitionById(
        Guid id, CancellationToken ct)
    {
        var result = await _achievementService.GetDefinitionByIdAsync(id, ct);
        if (result == null)
            return NotFound(ApiResponse<AchievementDefinitionResponse>.Fail("Định nghĩa huy hiệu không tìm thấy."));
        return Ok(ApiResponse<AchievementDefinitionResponse>.Ok(result));
    }

    /// <summary>
    /// [Admin] Tạo mới một định nghĩa huy hiệu
    /// </summary>
    [HttpPost("definitions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AchievementDefinitionResponse>>> CreateDefinition(
        CreateAchievementDefinitionRequest request, CancellationToken ct)
    {
        var result = await _achievementService.CreateDefinitionAsync(request, ct);
        return CreatedAtAction(nameof(GetDefinitionById), new { id = result.Id }, 
            ApiResponse<AchievementDefinitionResponse>.Ok(result));
    }

    /// <summary>
    /// [Admin] Cập nhật một định nghĩa huy hiệu
    /// </summary>
    [HttpPut("definitions/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AchievementDefinitionResponse>>> UpdateDefinition(
        Guid id, UpdateAchievementDefinitionRequest request, CancellationToken ct)
    {
        var result = await _achievementService.UpdateDefinitionAsync(id, request, ct);
        if (result == null)
            return NotFound(ApiResponse<AchievementDefinitionResponse>.Fail("Định nghĩa huy hiệu không tìm thấy."));
        return Ok(ApiResponse<AchievementDefinitionResponse>.Ok(result));
    }

    /// <summary>
    /// [Admin] Xóa mềm một định nghĩa huy hiệu (soft delete)
    /// </summary>
    [HttpDelete("definitions/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDefinition(Guid id, CancellationToken ct)
    {
        var success = await _achievementService.DeleteDefinitionAsync(id, ct);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Định nghĩa huy hiệu không tìm thấy."));
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>
    /// [Admin] Phục hồi một định nghĩa huy hiệu đã xóa
    /// </summary>
    [HttpPatch("definitions/{id:guid}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreDefinition(Guid id, CancellationToken ct)
    {
        var success = await _achievementService.RestoreDefinitionAsync(id, ct);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Định nghĩa huy hiệu không tìm thấy hoặc chưa bị xóa."));
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>
    /// [Admin] Xóa vĩnh viễn một định nghĩa huy hiệu (hard delete)
    /// </summary>
    [HttpDelete("definitions/{id:guid}/hard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> HardDeleteDefinition(Guid id, CancellationToken ct)
    {
        try
        {
            var success = await _achievementService.HardDeleteDefinitionAsync(id, ct);
            if (!success)
                return NotFound(ApiResponse<bool>.Fail("Định nghĩa huy hiệu không tìm thấy."));
            return Ok(ApiResponse<bool>.Ok(true));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(value);
    }
}