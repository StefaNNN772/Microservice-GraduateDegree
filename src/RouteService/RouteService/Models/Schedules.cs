using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RouteService.Models
{
    public class Schedules
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string BusLineId { get; set; }

        [ForeignKey("Providers")]
        public long ProviderId { get; set; }
        //public virtual Provider Provider { get; set; }

        [Required]
        public string Departure { get; set; }

        [Required]
        public string Arrival { get; set; }

        [Required]
        public TimeSpan DepartureTime { get; set; }

        [Required]
        public TimeSpan ArrivalTime { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public int PricePerKilometer { get; set; }

        [Required]
        public int AvailableSeats { get; set; }

        [Required]
        public string Days { get; set; }

        [Required]
        public int Discount { get; set; }
    }
}
