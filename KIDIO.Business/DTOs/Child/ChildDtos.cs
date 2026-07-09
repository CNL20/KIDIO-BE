namespace KIDIO.Business.DTOs.Child;

public record CreateChildRequest(
    string Name,
    int Age,
    string? AvatarUrl,
    string? StartingLevel  // "No" | "A little" | "Yes" → Beginner | Elementary | PreIntermediate
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
    bool IsRecommendedAge,
    string StartingLevel    // trả về string cho Frontend dễ đọc
);

public record ChildSummaryResponse(
    Guid Id,
    string Name,
    int Age,
    string? AvatarUrl,
    int TotalStars,
    int CurrentStreakDays
);

public record AddStarsRequest(
    int Stars,
    string Reason
);

public record AddStarsResponse(
    Guid ChildId,
    string ChildName,
    int StarsAdded,
    int TotalStars
);