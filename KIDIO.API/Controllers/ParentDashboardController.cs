using KIDIO.Business.DTOs.Dashboard;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParentDashboardController : ControllerBase
{
    private readonly IParentDashboardService _dashboardService;
    private readonly ILogger<ParentDashboardController> _logger;

    public ParentDashboardController(
        IParentDashboardService dashboardService,
        ILogger<ParentDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<ParentDashboardOverviewResponse>>> GetOverview(
        [FromQuery] int weeks = 4,
        CancellationToken ct = default)
    {
        // ExceptionMiddleware xử lý tất cả exception loại — không cần try/catch thủ công ở đây.
        // Trước đây catch (AppException) → BadRequest(400) sẽ nuốt mất NotFoundException(404).
        var result = await _dashboardService.GetOverviewAsync(GetCurrentUserId(), weeks, ct);
        return Ok(ApiResponse<ParentDashboardOverviewResponse>.Ok(result));
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(value);
    }
}
