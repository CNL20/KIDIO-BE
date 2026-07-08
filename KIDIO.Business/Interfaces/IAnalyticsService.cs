using KIDIO.Business.DTOs.Analytics;

namespace KIDIO.Business.Interfaces;

public interface IAnalyticsService
{
    Task<ParentAnalyticsResponse> GetParentAnalyticsAsync(CancellationToken ct = default);
    Task<RevenueAnalyticsResponse> GetRevenueAnalyticsAsync(int recentTransactionsCount = 20, CancellationToken ct = default);
}
