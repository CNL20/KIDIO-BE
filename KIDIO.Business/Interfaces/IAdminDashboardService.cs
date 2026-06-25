using KIDIO.Business.DTOs.Dashboard;

namespace KIDIO.Business.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardOverviewResponse> GetOverviewAsync(CancellationToken ct = default);
    Task<AdminDashboardDetailResponse> GetDetailAsync(int recentUsersCount = 10, int topLessonsCount = 10, CancellationToken ct = default);
}
