using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RouteService.Models
{
    public class BusLine
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ScheduleId { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual Schedules Schedule { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        [Required]
        public int AvailableSeats { get; set; }
    }
}
