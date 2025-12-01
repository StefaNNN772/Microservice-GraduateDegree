namespace ReservationService.DTOs
{
    public class PaymentIntentRequestDTO
    {
        public int Amount { get; set; }
        public string Currency { get; set; } = "rsd";
    }
}
