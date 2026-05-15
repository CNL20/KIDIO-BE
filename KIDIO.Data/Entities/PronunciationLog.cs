using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common;

namespace KIDIO.Data.Entities
{
    public class PronunciationLog : BaseEntity
    {
        public string TargetText { get; set; } = string.Empty;
        public string? AudioStorageUrl { get; set; }
        public int AccuracyScore { get; set; }          // 0-100
        public int FluencyScore { get; set; }
        public int CompletenessScore { get; set; }
        public string? AiFeedbackJson { get; set; }

        public Guid ChildId { get; set; }
        public Child Child { get; set; } = null!;

        public Guid? LessonId { get; set; }
        public Lesson? Lesson { get; set; }
    }
}
