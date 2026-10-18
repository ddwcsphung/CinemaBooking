using CinemaBooking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext(options)
    {
        public DbSet<Genre> Genres { get; set; } = null!;

        public DbSet<Movie> Movies { get; set; } = null!;

        public DbSet<Room> Rooms { get; set; } = null!;

        public DbSet<Seat> Seats { get; set; } = null!;

        public DbSet<Showtime> Showtimes { get; set; } = null!;

        public DbSet<Booking> Bookings { get; set; } = null!;

        public DbSet<BookingSeat> BookingSeats { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Seat>()
                .HasIndex(s => new
                {
                    s.RoomId,
                    s.RowLabel,
                    s.SeatNumber
                })
                .IsUnique();

            builder.Entity<Seat>()
                .HasOne(s => s.Room)
                .WithMany(r => r.Seats)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Showtime>()
                .Property(s => s.BasePrice)
                .HasPrecision(18, 2);

            builder.Entity<Showtime>()
                .HasOne(s => s.Movie)
                .WithMany()
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Showtime>()
                .HasOne(s => s.Room)
                .WithMany()
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Showtime>()
                .HasIndex(s => new { s.RoomId, s.StartTime });

            builder.Entity<Booking>()
                .HasIndex(b => b.BookingCode)
                .IsUnique();

            builder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Showtime)
                .WithMany()
                .HasForeignKey(b => b.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasIndex(b => new
                {
                    b.UserId,
                    b.CreatedAtUtc
                });

            builder.Entity<Booking>()
                .HasAlternateKey(b => new
                {
                    b.Id,
                    b.ShowtimeId
                });

            builder.Entity<BookingSeat>()
                .Property(bs => bs.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<BookingSeat>()
                .HasIndex(bs => new
                {
                    bs.ShowtimeId,
                    bs.SeatId
                })
                .IsUnique();

            builder.Entity<BookingSeat>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingSeats)
                .HasForeignKey(bs => new
                {
                    bs.BookingId,
                    bs.ShowtimeId
                })
                .HasPrincipalKey(b => new
                {
                    b.Id,
                    b.ShowtimeId
                })
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BookingSeat>()
                .HasOne(bs => bs.Seat)
                .WithMany()
                .HasForeignKey(bs => bs.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
