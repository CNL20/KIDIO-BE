using KIDIO.Common;
using KIDIO.Data.Entities;

public class Achievement : BaseEntity
{
    public Guid ChildId { get; set; }
    public Child Child { get; set; } = null!;

    public Guid AchievementDefinitionId { get; set; }
    public AchievementDefinition AchievementDefinition { get; set; } = null!;

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}