using System;
using KIDIO.Common;

namespace KIDIO.Data.Entities
{
    public class SubscriptionPlan : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
