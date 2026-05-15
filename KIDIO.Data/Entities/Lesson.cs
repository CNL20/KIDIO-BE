using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common.Enums;
using KIDIO.Common;

namespace KIDIO.Data.Entities
{
    public class Lesson : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LessonType Type { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public SkillType SkillFocus { get; set; }
        public int DurationSeconds { get; set; }        // ~3-5 phút
        public string? ThumbnailUrl { get; set; }
        public string? ContentJson { get; set; }        // structured content
        public string? AudioUrl { get; set; }
        public string? VideoUrl { get; set; }
        public int OrderIndex { get; set; }
        public bool IsPublished { get; set; } = false;

        // FK
        public Guid TopicId { get; set; }
        public Topic Topic { get; set; } = null!;

        // Navigation
        public ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
        public ICollection<Vocabulary> Vocabularies { get; set; } = new List<Vocabulary>();
    }
}
