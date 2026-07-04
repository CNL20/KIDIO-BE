namespace KIDIO.Business.DTOs.Payment
{
    public class PaymentResponseDTO
    {
        public string CheckoutUrl { get; set; } = string.Empty;
        public long OrderCode { get; set; }
    }
}
