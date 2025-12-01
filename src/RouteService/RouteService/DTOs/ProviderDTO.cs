using RouteService.Models;
using System.ComponentModel.DataAnnotations;

namespace RouteService.DTOs
{
    public class ProviderDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }
    }
}
