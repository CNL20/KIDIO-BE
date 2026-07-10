using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common;
using KIDIO.Common.Enums;

namespace KIDIO.Data.Entities
{
    public class Topic : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;

        // --- VIP / Placement access control ---
        public AccessType Access { get; set; } = AccessType.Free;
        public int LevelNumber { get; set; } = 1;

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
