using KIDIO.Business.DTOs.Achievement;

namespace KIDIO.Business.DTOs.Progress;

public record SubmitProgressRequest(
    Guid ChildId,
    Guid LessonId,
    int ScorePercent,       // 0-100
    int TimeSpentSeconds
);

public record ProgressResponse(
    Guid Id,
    Guid ChildId,
    string ChildName,
    Guid LessonId,
    string LessonTitle,
    bool IsCompleted,
    int StarsEarned,
    int ScorePercent,
    int TimeSpentSeconds,
    int AttemptCount,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    List<AchievementResponse> NewAchievements
);

public record ChildProgressSummary(
    Guid ChildId,
    string ChildName,
    int TotalLessonsCompleted,
    int TotalStars,
    int CurrentStreakDays,
    DateTime? LastLessonAt,
    List<TopicProgressItem> TopicProgresses
);

public record TopicProgressItem(
    Guid TopicId,
    string TopicName,
    int TotalLessons,
    int CompletedLessons,
    int ProgressPercent
);