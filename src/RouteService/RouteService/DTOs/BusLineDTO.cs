namespace RouteService.DTOs
{
    public class BusLineDTO
    {
        public long Id { get; set; }

        public long ScheduleId { get; set; }

        public string BusLineId { get; set; }

        public string Departure { get; set; }

        public string Arrival { get; set; }

        public string DepartureDate { get; set; }

        public string DepartureTime { get; set; }

        public string ArrivalTime { get; set; }

        public int AvailableSeats { get; set; }

        public int Price { get; set; }

        public string Provider { get; set; }

        public int Discount { get; set; }
    }
}
