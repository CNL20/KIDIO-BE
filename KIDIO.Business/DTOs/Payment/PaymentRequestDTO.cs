using System;

namespace KIDIO.Business.DTOs.Payment
{
    public class PaymentRequestDTO
    {
        public Guid SubscriptionPlanId { get; set; }
        public string CancelUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
    }
}
