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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FavouriteRoute>()
                .HasIndex(fr => fr.UserId);

            modelBuilder.Entity<FavouriteRoute>()
                .Property(fr => fr.UserId)
                .IsRequired();

            modelBuilder.Entity<Schedules>()
                .HasIndex(s => s.ProviderId);

            modelBuilder.Entity<Schedules>()
                .Property(s => s.ProviderId)
                .IsRequired();

            //modelBuilder.Entity<BusLine>()
            //    .HasOne<Schedules>()
            //    .WithMany()
            //    .HasForeignKey(bl => bl.ScheduleId)
            //    .OnDelete(DeleteBehavior.Cascade);

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
