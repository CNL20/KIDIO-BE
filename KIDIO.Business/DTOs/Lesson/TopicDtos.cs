namespace KIDIO.Business.DTOs.Lesson;

public record CreateTopicRequest(
    string Name,
    string? Description,
    string? IconUrl,
    int OrderIndex,
    string? Access,         // "Free" | "Premium", default Free
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be at least 1")]
    int? LevelNumber,       // Default 1
    bool? IsActive = true
);

public record UpdateTopicRequest(
    string Name,
    string? Description,
    string? IconUrl,
    int OrderIndex,
    bool IsActive,
    string? Access,         // "Free" | "Premium"
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be at least 1")]
    int? LevelNumber        // Default 1
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
    int LevelNumber         // e.g. 1, 2, 3...
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
    int LevelNumber,        // e.g. 1, 2, 3...
    bool IsUnlocked         // computed securely by backend
);