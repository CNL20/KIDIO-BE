using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KIDIO.Business.DTOs.Payment;
using KIDIO.Business.Interfaces;
using KIDIO.Common.Enums;
using KIDIO.Data.Entities;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace KIDIO.Business.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly KidioDbContext _context;
        private readonly PayOSClient _payOS;

        public PaymentService(KidioDbContext context, PayOSClient payOS)
        {
            _context = context;
            _payOS = payOS;
        }

        public async Task<PaymentResponseDTO> CreatePaymentUrlAsync(Guid userId, PaymentRequestDTO request)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(request.SubscriptionPlanId);
            if (plan == null || !plan.IsActive)
            {
                throw new Exception("Subscription plan not found or inactive.");
            }

            var orderCode = long.Parse(DateTimeOffset.Now.ToString("yyMMddHHmmss") + new Random().Next(100, 999).ToString());
            
            var transaction = new KIDIO.Data.Entities.PaymentTransaction
            {
                UserId = userId,
                SubscriptionPlanId = plan.Id,
                OrderCode = orderCode,
                Amount = plan.Price,
                Status = PaymentStatus.Pending,
                PaymentMethod = PaymentMethod.PayOS
            };

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var items = new List<PaymentLinkItem>
            {
                new PaymentLinkItem { Name = plan.Name, Quantity = 1, Price = (int)plan.Price }
            };

            var createPaymentReq = new CreatePaymentLinkRequest 
            {
                OrderCode = orderCode,
                Amount = (int)plan.Price,
                Description = $"KIDIO {plan.Name}",
                Items = items,
                CancelUrl = request.CancelUrl,
                ReturnUrl = request.ReturnUrl
            };

            var createPayment = await _payOS.PaymentRequests.CreateAsync(createPaymentReq);

            return new PaymentResponseDTO
            {
                CheckoutUrl = createPayment.CheckoutUrl,
                OrderCode = orderCode
            };
        }

        public async Task<TransactionHistoryDTO> VerifyWebhookAsync(Webhook webhook)
        {
            try
            {
                var webhookData = await _payOS.Webhooks.VerifyAsync(webhook);

                if (webhookData.Code == "00")
                {
                    var transaction = await _context.PaymentTransactions
                        .Include(x => x.SubscriptionPlan)
                        .Include(x => x.User)
                        .FirstOrDefaultAsync(x => x.OrderCode == webhookData.OrderCode);

                    if (transaction != null && transaction.Status == PaymentStatus.Pending)
                    {
                        transaction.Status = PaymentStatus.Success;
                        transaction.PaymentDate = DateTime.UtcNow;
                        transaction.ProviderTransactionId = webhookData.Reference;

                        if (transaction.User.PremiumExpiryDate == null || transaction.User.PremiumExpiryDate < DateTime.UtcNow)
                        {
                            transaction.User.PremiumExpiryDate = DateTime.UtcNow.AddDays(transaction.SubscriptionPlan.DurationInDays);
                        }
                        else
                        {
                            transaction.User.PremiumExpiryDate = transaction.User.PremiumExpiryDate.Value.AddDays(transaction.SubscriptionPlan.DurationInDays);
                        }

                        await _context.SaveChangesAsync();
                    }
                    
                    if (transaction != null)
                    {
                         return new TransactionHistoryDTO
                        {
                            Id = transaction.Id,
                            Amount = transaction.Amount,
                            Status = transaction.Status,
                            PaymentMethod = transaction.PaymentMethod
                        };
                    }
                    
                    // Nếu không tìm thấy OrderCode (Ví dụ: Webhook test của PayOS)
                    // Ghi log và kết thúc bình thường, KHÔNG ném exception để tránh lỗi 400
                    Console.WriteLine($"[Webhook] OrderCode {webhookData.OrderCode} not found. This might be a test ping from PayOS.");
                    return null!;
                }
                
                throw new Exception($"Webhook verification failed with code: {webhookData.Code}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing webhook: {ex.Message}");
            }
        }

        public async Task<IEnumerable<TransactionHistoryDTO>> GetTransactionHistoryAsync(Guid userId)
        {
            return await _context.PaymentTransactions
                .Include(x => x.SubscriptionPlan)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TransactionHistoryDTO
                {
                    Id = x.Id,
                    PlanName = x.SubscriptionPlan.Name,
                    Amount = x.Amount,
                    Status = x.Status,
                    PaymentMethod = x.PaymentMethod,
                    PaymentDate = x.PaymentDate,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }
    }
}
