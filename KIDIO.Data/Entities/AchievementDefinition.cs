using KIDIO.Common;

namespace KIDIO.Data.Entities;

public class AchievementDefinition : BaseEntity
{
    public string Type { get; set; } = string.Empty;        // Stars, Streak, Lessons
    public int Threshold { get; set; }                       // 10, 30, 50, 100 (ngưỡng)
    public string Name { get; set; } = string.Empty;         // "Star Collector", "Week Warrior"
    public string? Description { get; set; }                 // Mô tả chi tiết
    public string? BadgeUrl { get; set; }                    // Link icon/badge
    public int OrderIndex { get; set; } = 0;                // Thứ tự hiển thị
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
}
