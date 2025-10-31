using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ReservationService.Models
{
    public class Ticket
    {
        [Key]
        public long Id { get; set; }

        [Required, NotNull]
        public long BusLineId { get; set; }

        //[ForeignKey("BusLineId")]
        //public virtual BusLine BusLine { get; set; }

        [Required, NotNull]
        public long UserId { get; set; }

        //[ForeignKey("UserId")]
        //public virtual User User { get; set; }

        [Required, NotNull]
        public string QRCodeValue { get; set; }

        [Required, NotNull]
        public int NumberOfSeats { get; set; }

        [Required]
        public bool IsChecked { get; set; }

        [Required]
        public DateTime PurchaseTime { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public bool IsPaid { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }
}
