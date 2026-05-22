using KIDIO.Common;

namespace KIDIO.Data.Entities;

public class Achievement : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BadgeUrl { get; set; }
    public string AchievementType { get; set; } = string.Empty; // Stars, Streak, Lessons, Topic
    public int Threshold { get; set; }      // milestone đạt được, ví dụ: 10 sao, 7 ngày streak
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    public Guid ChildId { get; set; }
    public Child Child { get; set; } = null!;
}