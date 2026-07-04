using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KIDIO.Business.DTOs.Payment;
using PayOS.Models.Webhooks;

namespace KIDIO.Business.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> CreatePaymentUrlAsync(Guid userId, PaymentRequestDTO request);
        Task<TransactionHistoryDTO> VerifyWebhookAsync(Webhook webhook);
        Task<IEnumerable<TransactionHistoryDTO>> GetTransactionHistoryAsync(Guid userId);
    }
}
