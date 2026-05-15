using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common;

namespace KIDIO.Data.Entities
{
    public class LessonProgress : BaseEntity
    {
        public bool IsCompleted { get; set; } = false;
        public int StarsEarned { get; set; } = 0;       // 0-3
        public int ScorePercent { get; set; } = 0;
        public int TimeSpentSeconds { get; set; } = 0;
        public DateTime? CompletedAt { get; set; }
        public int AttemptCount { get; set; } = 1;

        // FK
        public Guid ChildId { get; set; }
        public Child Child { get; set; } = null!;

        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;
    }
}
