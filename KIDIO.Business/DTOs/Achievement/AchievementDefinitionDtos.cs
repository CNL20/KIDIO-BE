namespace KIDIO.Business.DTOs.Achievement;

public record AchievementDefinitionResponse(
    Guid Id,
    string Type,
    int Threshold,
    string Name,
    string? Description,
    string? BadgeUrl,
    int OrderIndex,
    bool IsActive
);

public record CreateAchievementDefinitionRequest(
    string Type,
    int Threshold,
    string Name,
    string? Description,
    string? BadgeUrl,
    int OrderIndex = 0
);

public record UpdateAchievementDefinitionRequest(
    string Type,
    int Threshold,
    string Name,
    string? Description,
    string? BadgeUrl,
    int OrderIndex,
    bool IsActive
);
