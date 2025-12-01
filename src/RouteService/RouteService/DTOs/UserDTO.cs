using RouteService.Models;
using System.ComponentModel.DataAnnotations;

namespace RouteService.DTOs
{
    public class UserDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
    }
}
