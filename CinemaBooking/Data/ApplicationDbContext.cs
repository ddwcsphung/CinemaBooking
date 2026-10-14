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
        }
    }
}