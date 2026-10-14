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
            // Giữ cấu hình các bảng tài khoản và vai trò của Identity.
            base.OnModelCreating(builder);

            // Không cho phép trùng vị trí ghế trong cùng phòng.
            builder.Entity<Seat>()
                .HasIndex(s => new
                {
                    s.RoomId,
                    s.RowLabel,
                    s.SeatNumber
                })
                .IsUnique();

            // Một phòng có nhiều ghế.
            // Không cho xóa phòng khi vẫn còn ghế.
            builder.Entity<Seat>()
                .HasOne(s => s.Room)
                .WithMany(r => r.Seats)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kiểu dữ liệu tiền trong SQL Server.
            builder.Entity<Showtime>()
                .Property(s => s.BasePrice)
                .HasPrecision(18, 2);

            // Mỗi suất chiếu thuộc một phim.
            // Không xóa phim khi còn suất chiếu tham chiếu tới.
            builder.Entity<Showtime>()
                .HasOne(s => s.Movie)
                .WithMany()
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mỗi suất chiếu thuộc một phòng.
            // Không xóa phòng khi còn suất chiếu tham chiếu tới.
            builder.Entity<Showtime>()
                .HasOne(s => s.Room)
                .WithMany()
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hỗ trợ truy vấn lịch chiếu theo phòng và thời gian.
            builder.Entity<Showtime>()
                .HasIndex(s => new { s.RoomId, s.StartTime });

            // ==================== BOOKING ====================

            // Mã đặt vé không được trùng.
            builder.Entity<Booking>()
                .HasIndex(b => b.BookingCode)
                .IsUnique();

            // Kiểu dữ liệu tiền.
            builder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            // Một người dùng có nhiều đơn.
            // Không xóa người dùng khi vẫn còn đơn tham chiếu.
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Một suất chiếu có nhiều đơn.
            // Không xóa suất chiếu đã có đơn.
            builder.Entity<Booking>()
                .HasOne(b => b.Showtime)
                .WithMany()
                .HasForeignKey(b => b.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hỗ trợ truy vấn lịch sử đặt vé của người dùng.
            builder.Entity<Booking>()
                .HasIndex(b => new
                {
                    b.UserId,
                    b.CreatedAtUtc
                });

            // Khóa để BookingSeat tham chiếu đồng thời
            // đến mã đơn và suất chiếu của chính đơn đó.
            builder.Entity<Booking>()
                .HasAlternateKey(b => new
                {
                    b.Id,
                    b.ShowtimeId
                });

            // ==================== BOOKING SEAT ====================

            builder.Entity<BookingSeat>()
                .Property(bs => bs.UnitPrice)
                .HasPrecision(18, 2);

            // Ràng buộc quan trọng:
            // một ghế chỉ được đặt một lần trong cùng suất chiếu.
            builder.Entity<BookingSeat>()
                .HasIndex(bs => new
                {
                    bs.ShowtimeId,
                    bs.SeatId
                })
                .IsUnique();

            // Một đơn có nhiều ghế.
            // Khóa ngoại ghép bảo đảm suất chiếu ở dòng ghế
            // luôn trùng với suất chiếu của đơn.
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

            // Không xóa ghế vật lý khi ghế đó đã có lịch sử đặt.
            builder.Entity<BookingSeat>()
                .HasOne(bs => bs.Seat)
                .WithMany()
                .HasForeignKey(bs => bs.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}