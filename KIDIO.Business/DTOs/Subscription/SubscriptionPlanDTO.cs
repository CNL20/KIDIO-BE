using System;

namespace KIDIO.Business.DTOs.Subscription
{
    public class SubscriptionPlanDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public int DisplayOrder { get; set; }
    }
}
