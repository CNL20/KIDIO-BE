namespace KIDIO.Business.DTOs.Dashboard;

public record ParentDashboardChildItemResponse(
    Guid ChildId,
    string ChildName,
    int Age,
    string? AvatarUrl,
    int CompletedLessons,
    int TotalStars,
    int CurrentStreakDays,
    int TimeSpentSeconds,
    int CompletionPercent,
    DateTime? LastLessonAt
);

public record WeeklyProgressResponse(
    DateTime WeekStart,
    DateTime WeekEnd,
    int CompletedLessons,
    int TimeSpentSeconds,
    int ActiveChildrenCount
);

public record ChildComparisonResponse(
    Guid ChildId,
    string ChildName,
    int CompletedLessons,
    int TotalStars,
    int TimeSpentSeconds,
    int Rank
);

public record ParentDashboardOverviewResponse(
    Guid ParentId,
    string ParentName,
    int TotalChildren,
    int TotalPublishedLessons,
    int TotalLessonsCompleted,
    int TotalStars,
    int TotalTimeSpentSeconds,
    DateTime GeneratedAt,
    List<ParentDashboardChildItemResponse> Children,
    List<WeeklyProgressResponse> WeeklyProgress,
    List<ChildComparisonResponse> Comparisons
);
