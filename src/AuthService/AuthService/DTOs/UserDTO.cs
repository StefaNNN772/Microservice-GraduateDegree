using AuthService.Models;

namespace AuthService.DTOs
{
    public class UserDTO
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string? LastName { get; set; }
        public DiscountType DiscountType { get; set; }
        public DiscountStatus DiscountStatus { get; set; }
        public string? ProfileImagePath { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
