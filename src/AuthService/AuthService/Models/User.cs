using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime Birthday { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public DiscountType? DiscountType { get; set; }

        [Required]
        public DiscountStatus DiscountStatus { get; set; }

        [Required]
        public DateTime DiscountValidUntil { get; set; }

        public string? DiscountDocumentPath { get; set; }

        public string? ProfileImagePath { get; set; }

        //public virtual ICollection<FavouriteRoute> FavouriteRoutes { get; set; }
    }
}
