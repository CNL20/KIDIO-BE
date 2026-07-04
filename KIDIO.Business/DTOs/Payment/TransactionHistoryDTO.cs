using System;
using KIDIO.Common.Enums;

namespace KIDIO.Business.DTOs.Payment
{
    public class TransactionHistoryDTO
    {
        public Guid Id { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
