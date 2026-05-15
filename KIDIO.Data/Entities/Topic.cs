using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common;

namespace KIDIO.Data.Entities
{
    public class Topic : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
