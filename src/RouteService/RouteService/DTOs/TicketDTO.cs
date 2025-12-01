using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RouteService.DTOs
{
    public class TicketDTO
    {
        public long Id { get; set; }

        public long BusLineId { get; set; }

        public long UserId { get; set; }

        public string QRCodeValue { get; set; }

        public int NumberOfSeats { get; set; }

        public bool IsChecked { get; set; }

        public DateTime PurchaseTime { get; set; }

        public int Price { get; set; }

        public bool IsPaid { get; set; }

        public string PaymentMethod { get; set; }

        public string UserEmail { get; set; }

        public string Departure { get; set; }

        public string Arrival { get; set; }

        public string DepartureTime { get; set; }

        public string ArrivalTime { get; set; }

        public string DepartureDate { get; set; }
    }
}
