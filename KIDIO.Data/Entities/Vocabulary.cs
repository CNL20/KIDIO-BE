using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common;

namespace KIDIO.Data.Entities
{
    public class Vocabulary : BaseEntity
    {
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string? PhoneticText { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? ExampleSentence { get; set; }

        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;
    }
}
