using KIDIO.Business.DTOs.Dashboard;

namespace KIDIO.Business.DTOs.Analytics;

// ── Parent Analytics ──────────────────────────────────────

public record ParentAnalyticsResponse(
    // Overview
    int TotalParents,
    int ActiveParents,          // Có con đã học bài trong 30 ngày gần đây
    int NewParentsThisMonth,
    double AvgChildrenPerParent,
    // Charts
    List<ChartPoint> ParentRegistrationTrend,  // Đăng ký theo tháng
    List<ChartPoint> ParentActivityTrend,      // Số parent có con active theo 7 ngày
    // Tables
    List<TopActiveParentResponse> TopActiveParents
);

public record TopActiveParentResponse(
    Guid ParentId,
    string DisplayName,
    string Email,
    int ChildrenCount,
    int TotalLessonsCompleted,
    int TotalStars,
    DateTime LastActiveAt
);

// ── Revenue Analytics ─────────────────────────────────────

public record RevenueAnalyticsResponse(
    // Overview
    decimal TotalRevenue,
    decimal RevenueThisMonth,
    int TotalTransactions,
    int SuccessfulTransactions,
    // Charts
    List<ChartPoint> MonthlyRevenue,
    List<RevenueByPlanResponse> RevenueByPlan,
    List<ChartPoint> RevenueByMethod,
    // Tables
    List<TransactionDetailResponse> RecentTransactions
);

public record RevenueByPlanResponse(
    Guid PlanId,
    string PlanName,
    decimal TotalRevenue,
    int TransactionCount
);

public record TransactionDetailResponse(
    Guid Id,
    long OrderCode,
    string UserDisplayName,
    string UserEmail,
    string PlanName,
    decimal Amount,
    string Status,
    string PaymentMethod,
    DateTime? PaymentDate,
    DateTime CreatedAt
);
