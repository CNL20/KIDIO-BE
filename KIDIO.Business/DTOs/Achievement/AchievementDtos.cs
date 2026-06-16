namespace KIDIO.Business.DTOs.Achievement;

public record AchievementResponse(
    Guid Id,
    string Name,
    string? Description,
    string? BadgeUrl,
    string AchievementType,
    int Threshold,
    DateTime EarnedAt
);

// Trả về khi submit progress — thông báo achievement mới unlock
public record AchievementUnlockResult(
    List<AchievementResponse> NewAchievements,
    bool HasNew
);