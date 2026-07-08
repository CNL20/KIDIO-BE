using KIDIO.Business.DTOs.Analytics;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AdminAnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get parent analytics (overview, trends, top active parents)
    /// </summary>
    [HttpGet("parents")]
    public async Task<ActionResult<ApiResponse<ParentAnalyticsResponse>>> GetParentAnalytics(
        CancellationToken ct = default)
    {
        var result = await _analyticsService.GetParentAnalyticsAsync(ct);
        return Ok(ApiResponse<ParentAnalyticsResponse>.Ok(result));
    }

    /// <summary>
    /// Get revenue analytics (overview, trends, transactions)
    /// </summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<RevenueAnalyticsResponse>>> GetRevenueAnalytics(
        [FromQuery] int recentTransactionsCount = 20,
        CancellationToken ct = default)
    {
        var result = await _analyticsService.GetRevenueAnalyticsAsync(recentTransactionsCount, ct);
        return Ok(ApiResponse<RevenueAnalyticsResponse>.Ok(result));
    }
}
