namespace KIDIO.Business.DTOs.Lesson;

public record CreateTopicRequest(
    string Name,
    string? Description,
    string? IconUrl,
    int OrderIndex
);

public record UpdateTopicRequest(
    string Name,
    string? Description,
    string? IconUrl,
    int OrderIndex,
    bool IsActive
);

public record TopicResponse(
    Guid Id,
    string Name,
    string? Description,
    string? IconUrl,
    int OrderIndex,
    bool IsActive,
    int TotalLessons,
    DateTime CreatedAt
);

public record TopicSummaryResponse(
    Guid Id,
    string Name,
    string? IconUrl,
    int OrderIndex,
    int TotalLessons
);