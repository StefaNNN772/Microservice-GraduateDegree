namespace ReservationService.DTOs
{
    public class PaymentIntentResponseDTO
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
