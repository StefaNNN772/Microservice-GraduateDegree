using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReservationService.Models
{
    public class ReservedSeat
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long BusLineId { get; set; }

        //[ForeignKey("BusLineId")]
        //public virtual BusLine BusLine { get; set; }

        [Required]
        public int SeatNumber { get; set; }

        [Required]
        public long TicketId { get; set; }
    }
}
