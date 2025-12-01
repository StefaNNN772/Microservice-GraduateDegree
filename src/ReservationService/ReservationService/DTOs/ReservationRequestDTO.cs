namespace ReservationService.DTOs
{
    public class ReservationRequestDTO
    {
        public long Id { get; set; }
        public int NumberOfPassengers { get; set; }
        public string PaymentMethod { get; set; }
        public int Price { get; set; }
    }
}
