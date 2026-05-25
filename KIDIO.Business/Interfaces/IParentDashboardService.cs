using KIDIO.Business.DTOs.Dashboard;

namespace KIDIO.Business.Interfaces;

public interface IParentDashboardService
{
    Task<ParentDashboardOverviewResponse> GetOverviewAsync(
        Guid parentId,
        int weeks = 4,
        CancellationToken ct = default);
}
