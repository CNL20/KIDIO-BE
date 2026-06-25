namespace KIDIO.Business.DTOs.Dashboard;

public record AdminDashboardOverviewResponse(
    int TotalParents,
    int TotalChildren,
    int TotalTopics,
    int TotalLessons,
    int TotalPublishedLessons,
    int TotalUnpublishedLessons,
    int TotalLessonCompletions,
    int TotalVocabularies,
    int TotalAchievementsEarned,
    DateTime GeneratedAt
);

public record AdminRecentUserResponse(
    Guid UserId,
    string DisplayName,
    string Email,
    string Role,
    DateTime CreatedAt
);

public record AdminTopLessonResponse(
    Guid LessonId,
    string Title,
    string TopicName,
    int CompletionCount,
    double AvgScorePercent
);

public record AdminDashboardDetailResponse(
    AdminDashboardOverviewResponse Overview,
    List<AdminRecentUserResponse> RecentUsers,
    List<AdminTopLessonResponse> TopLessons
);
