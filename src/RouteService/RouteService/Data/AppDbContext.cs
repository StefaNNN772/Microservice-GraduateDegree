using Microsoft.EntityFrameworkCore;
using RouteService.Models;
using System.Net.Sockets;

namespace RouteService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Schedules> Schedules { get; set; }
        public DbSet<BusLine> BusLines { get; set; }
        public DbSet<FavouriteRoute> FavouriteRoutes { get; set; }
    }
}
