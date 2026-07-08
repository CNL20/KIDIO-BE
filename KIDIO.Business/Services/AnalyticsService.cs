using KIDIO.Business.DTOs.Analytics;
using KIDIO.Business.DTOs.Dashboard;
using KIDIO.Business.Interfaces;
using KIDIO.Common.Enums;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _uow;

    public AnalyticsService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ParentAnalyticsResponse> GetParentAnalyticsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Overview
        var totalParents = await _uow.Users.Query()
            .CountAsync(u => u.Role == UserRole.Parent, ct);

        var newParentsThisMonth = await _uow.Users.Query()
            .CountAsync(u => u.Role == UserRole.Parent && u.CreatedAt >= firstDayOfMonth, ct);

        // Active parents = parents có ít nhất 1 con đã học bài trong 30 ngày
        var activeParentIds = await _uow.Children.Query()
            .Where(c => c.LastLessonAt.HasValue && c.LastLessonAt.Value >= thirtyDaysAgo)
            .Select(c => c.ParentId)
            .Distinct()
            .ToListAsync(ct);
        var activeParents = activeParentIds.Count;

        // Average children per parent
        var totalChildren = await _uow.Children.Query().CountAsync(ct);
        var avgChildrenPerParent = totalParents == 0 ? 0 : Math.Round((double)totalChildren / totalParents, 1);

        // Chart: Parent registration trend (theo tháng)
        var registrationData = await _uow.Users.Query()
            .Where(u => u.Role == UserRole.Parent)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        var parentRegistrationTrend = registrationData
            .Select(x => new ChartPoint($"{x.Month:D2}/{x.Year}", x.Count))
            .ToList();

        // Chart: Parent activity trend (7 ngày gần nhất)
        var sevenDaysAgo = now.AddDays(-7).Date;
        var childActivity = await _uow.Children.Query()
            .Where(c => c.LastLessonAt.HasValue && c.LastLessonAt.Value >= sevenDaysAgo)
            .Select(c => new { c.ParentId, c.LastLessonAt })
            .ToListAsync(ct);

        var parentActivityTrend = new List<ChartPoint>();
        for (int i = 0; i <= 7; i++)
        {
            var date = sevenDaysAgo.AddDays(i);
            var count = childActivity
                .Where(c => c.LastLessonAt!.Value.Date <= date)
                .Select(c => c.ParentId)
                .Distinct()
                .Count();
            parentActivityTrend.Add(new ChartPoint(date.ToString("dd/MM"), count));
        }

        // Table: Top active parents
        var parentIds = await _uow.Users.Query()
            .Where(u => u.Role == UserRole.Parent)
            .Select(u => u.Id)
            .ToListAsync(ct);

        var childrenByParent = await _uow.Children.Query()
            .Include(c => c.Progresses)
            .Where(c => parentIds.Contains(c.ParentId))
            .ToListAsync(ct);

        var topActiveParents = childrenByParent
            .GroupBy(c => c.ParentId)
            .Select(g =>
            {
                var totalLessonsCompleted = g.Sum(c => c.Progresses.Count(p => p.IsCompleted));
                var totalStarsSum = g.Sum(c => c.TotalStars);
                var lastActive = g.Max(c => c.LastLessonAt) ?? g.Max(c => c.CreatedAt);
                return new { ParentId = g.Key, ChildrenCount = g.Count(), TotalLessonsCompleted = totalLessonsCompleted, TotalStars = totalStarsSum, LastActiveAt = lastActive };
            })
            .OrderByDescending(x => x.TotalLessonsCompleted)
            .Take(10)
            .ToList();

        // Lấy thông tin user cho top parents
        var topParentUserIds = topActiveParents.Select(x => x.ParentId).ToList();
        var topParentUsers = await _uow.Users.Query()
            .Where(u => topParentUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var topActiveParentResponses = topActiveParents.Select(x =>
        {
            topParentUsers.TryGetValue(x.ParentId, out var user);
            return new TopActiveParentResponse(
                ParentId: x.ParentId,
                DisplayName: user?.DisplayName ?? "Unknown",
                Email: user?.Email ?? "Unknown",
                ChildrenCount: x.ChildrenCount,
                TotalLessonsCompleted: x.TotalLessonsCompleted,
                TotalStars: x.TotalStars,
                LastActiveAt: x.LastActiveAt
            );
        }).ToList();

        return new ParentAnalyticsResponse(
            TotalParents: totalParents,
            ActiveParents: activeParents,
            NewParentsThisMonth: newParentsThisMonth,
            AvgChildrenPerParent: avgChildrenPerParent,
            ParentRegistrationTrend: parentRegistrationTrend,
            ParentActivityTrend: parentActivityTrend,
            TopActiveParents: topActiveParentResponses
        );
    }

    public async Task<RevenueAnalyticsResponse> GetRevenueAnalyticsAsync(
        int recentTransactionsCount = 20, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Overview
        var allTransactions = await _uow.PaymentTransactions.Query().ToListAsync(ct);
        var successTransactions = allTransactions.Where(t => t.Status == PaymentStatus.Success).ToList();

        var totalRevenue = successTransactions.Sum(t => t.Amount);
        var revenueThisMonth = successTransactions
            .Where(t => t.CreatedAt >= firstDayOfMonth)
            .Sum(t => t.Amount);
        var totalTransactions = allTransactions.Count;
        var successfulTransactions = successTransactions.Count;

        // Chart: Monthly revenue
        var monthlyData = successTransactions
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = (int)g.Sum(t => t.Amount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        var monthlyRevenue = monthlyData
            .Select(x => new ChartPoint($"{x.Month:D2}/{x.Year}", x.Revenue))
            .ToList();

        // Chart: Revenue by plan
        var planIds = successTransactions.Select(t => t.SubscriptionPlanId).Distinct().ToList();
        var plans = await _uow.SubscriptionPlans.Query()
            .Where(p => planIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var revenueByPlan = successTransactions
            .GroupBy(t => t.SubscriptionPlanId)
            .Select(g =>
            {
                plans.TryGetValue(g.Key, out var plan);
                return new RevenueByPlanResponse(
                    PlanId: g.Key,
                    PlanName: plan?.Name ?? "Unknown",
                    TotalRevenue: g.Sum(t => t.Amount),
                    TransactionCount: g.Count()
                );
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();

        // Chart: Revenue by payment method
        var revenueByMethod = successTransactions
            .GroupBy(t => t.PaymentMethod.ToString())
            .Select(g => new ChartPoint(g.Key, (int)g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Value)
            .ToList();

        // Table: Recent transactions
        var recentTx = await _uow.PaymentTransactions.Query()
            .Include(t => t.User)
            .Include(t => t.SubscriptionPlan)
            .OrderByDescending(t => t.CreatedAt)
            .Take(recentTransactionsCount)
            .Select(t => new TransactionDetailResponse(
                t.Id,
                t.OrderCode,
                t.User.DisplayName,
                t.User.Email,
                t.SubscriptionPlan.Name,
                t.Amount,
                t.Status.ToString(),
                t.PaymentMethod.ToString(),
                t.PaymentDate,
                t.CreatedAt
            ))
            .ToListAsync(ct);

        return new RevenueAnalyticsResponse(
            TotalRevenue: totalRevenue,
            RevenueThisMonth: revenueThisMonth,
            TotalTransactions: totalTransactions,
            SuccessfulTransactions: successfulTransactions,
            MonthlyRevenue: monthlyRevenue,
            RevenueByPlan: revenueByPlan,
            RevenueByMethod: revenueByMethod,
            RecentTransactions: recentTx
        );
    }
}
