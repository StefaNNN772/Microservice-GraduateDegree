using Microsoft.EntityFrameworkCore;
using ReservationService.Models;

namespace ReservationService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<ReservedSeat> ReservedSeats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.UserId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.BusLineId);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.UserId)
                .IsRequired();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.BusLineId)
                .IsRequired();

            modelBuilder.Entity<ReservedSeat>()
                .HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(rs => rs.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReservedSeat>()
                .HasIndex(rs => rs.BusLineId);

            modelBuilder.Entity<ReservedSeat>()
                .Property(rs => rs.BusLineId)
                .IsRequired();

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
