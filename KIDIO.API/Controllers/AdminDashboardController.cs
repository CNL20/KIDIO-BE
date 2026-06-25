using KIDIO.Business.DTOs.Dashboard;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get overview statistics (total parents, children, lessons, etc.)
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<AdminDashboardOverviewResponse>>> GetOverview(
        CancellationToken ct = default)
    {
        var result = await _dashboardService.GetOverviewAsync(ct);
        return Ok(ApiResponse<AdminDashboardOverviewResponse>.Ok(result));
    }

    /// <summary>
    /// Get detailed dashboard with overview + recent users + top lessons
    /// </summary>
    [HttpGet("detail")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDetailResponse>>> GetDetail(
        [FromQuery] int recentUsersCount = 10,
        [FromQuery] int topLessonsCount = 10,
        CancellationToken ct = default)
    {
        var result = await _dashboardService.GetDetailAsync(recentUsersCount, topLessonsCount, ct);
        return Ok(ApiResponse<AdminDashboardDetailResponse>.Ok(result));
    }
}
