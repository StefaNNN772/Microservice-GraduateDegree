namespace ReservationService.DTOs
{
    public class ScheduleDTO
    {
        public long Id { get; set; }
        public long ProviderId { get; set; }
        public string BusLineId { get; set; }
        public string Departure { get; set; }

        public string Arrival { get; set; }

        public string DepartureTime { get; set; }

        public string ArrivalTime { get; set; }

        public int Price { get; set; }

        public int PricePerKilometer { get; set; }

        public int AvailableSeats { get; set; }
        public string Days { get; set; }

        public int Discount { get; set; }
    }
}
