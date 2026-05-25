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
        try
        {
            var result = await _dashboardService.GetOverviewAsync(GetCurrentUserId(), weeks, ct);
            return Ok(ApiResponse<ParentDashboardOverviewResponse>.Ok(result));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<ParentDashboardOverviewResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading parent dashboard overview");
            return StatusCode(500, ApiResponse<ParentDashboardOverviewResponse>.Fail("Internal server error."));
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(value);
    }
}
