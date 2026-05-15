using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common;
using KIDIO.Common.Enums;

namespace KIDIO.Data.Entities
{
    public class Child : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? AvatarUrl { get; set; }
        public int TotalStars { get; set; } = 0;
        public int CurrentStreakDays { get; set; } = 0;
        public DateTime? LastLessonAt { get; set; }

        // FK
        public Guid ParentId { get; set; }
        public User Parent { get; set; } = null!;

        // Navigation
        public ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
        public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
        public ICollection<PronunciationLog> PronunciationLogs { get; set; } = new List<PronunciationLog>();
    }
}
