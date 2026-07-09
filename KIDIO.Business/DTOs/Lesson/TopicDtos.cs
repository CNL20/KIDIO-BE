namespace KIDIO.Business.DTOs.Lesson;

public record CreateTopicRequest(
    string Name,
    string? Description,
    string? IconUrl,
    int OrderIndex,
    string? Access,         // "Free" | "Premium", default Free
    string? MinDifficulty,  // "Beginner" | "Elementary" | "PreIntermediate", default Beginner
    bool? IsActive = true
);

public record UpdateTopicRequest(
    string Name,
    string? Description,
    string? IconUrl,
    int OrderIndex,
    bool IsActive,
    string? Access,         // "Free" | "Premium"
    string? MinDifficulty   // "Beginner" | "Elementary" | "PreIntermediate"
);

public record TopicResponse(
    Guid Id,
    string Name,
    string? Description,
    string? IconUrl,
    int OrderIndex,
    bool IsActive,
    int TotalLessons,
    DateTime CreatedAt,
    string Access,          // "Free" | "Premium"
    string MinDifficulty    // "Beginner" | "Elementary" | "PreIntermediate"
);

public record TopicSummaryResponse(
    Guid Id,
    string Name,
    string? IconUrl,
    int OrderIndex,
    int TotalLessons,
    bool IsActive,
    DateTime CreatedAt,
    string Access,          // "Free" | "Premium"
    string MinDifficulty,   // "Beginner" | "Elementary" | "PreIntermediate"
    bool IsUnlocked         // computed: true nếu child.StartingLevel >= topic.MinDifficulty
);