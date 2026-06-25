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

public enum ActivityType
{
    LessonCompleted,
    LessonStarted,
    PronunciationScored,
    AchievementEarned
}

public record AdminRecentActivityResponse(
    Guid ChildId,
    string ChildName,
    string ActivityType,
    string Description,
    string? MetaValue,
    DateTime Timestamp
);

public record AdminDashboardDetailResponse(
    AdminDashboardOverviewResponse Overview,
    List<AdminRecentUserResponse> RecentUsers,
    List<AdminTopLessonResponse> TopLessons,
    List<AdminRecentActivityResponse> RecentActivities
);
