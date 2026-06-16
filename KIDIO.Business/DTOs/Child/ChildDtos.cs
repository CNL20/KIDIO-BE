namespace KIDIO.Business.DTOs.Child;

public record CreateChildRequest(
    string Name,
    int Age,
    string? AvatarUrl
);

public record UpdateChildRequest(
    string Name,
    int Age,
    string? AvatarUrl
);

public record ChildResponse(
    Guid Id,
    string Name,
    int Age,
    string? AvatarUrl,
    int TotalStars,
    int CurrentStreakDays,
    DateTime? LastLessonAt,
    DateTime CreatedAt,
    bool IsRecommendedAge
);

public record ChildSummaryResponse(
    Guid Id,
    string Name,
    int Age,
    string? AvatarUrl,
    int TotalStars,
    int CurrentStreakDays
);