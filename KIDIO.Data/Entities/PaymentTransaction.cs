using System;
using KIDIO.Common;
using KIDIO.Common.Enums;

namespace KIDIO.Data.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        
        // TransactionCode do hệ thống mình tự sinh (VD: mã đơn hàng int cho PayOS)
        public long OrderCode { get; set; } 
        
        public decimal Amount { get; set; }
        
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.PayOS;
        
        // Mã giao dịch từ phía PayOS/VNPay trả về
        public string? ProviderTransactionId { get; set; }
        
        public DateTime? PaymentDate { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    }
}
