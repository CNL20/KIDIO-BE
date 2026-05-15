using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common;

namespace KIDIO.Data.Entities
{
    public class Achievement : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BadgeUrl { get; set; }
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        public Guid ChildId { get; set; }
        public Child Child { get; set; } = null!;
    }
}
