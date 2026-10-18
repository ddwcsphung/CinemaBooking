using CinemaBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var password = configuration["DemoData:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Cần cấu hình DemoData:Password trong User Secrets trước khi bật dữ liệu demo.");
        }

        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        foreach (var name in new[] { "Admin", "Customer" })
        {
            if (!await roles.RoleExistsAsync(name))
            {
                Ensure(await roles.CreateAsync(new IdentityRole(name)));
            }
        }

        var accounts = new List<IdentityUser>();
        foreach (var email in new[] { "demo.admin@example.com", "demo.a@example.com", "demo.b@example.com" })
        {
            var account = await users.FindByEmailAsync(email);
            if (account == null)
            {
                account = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                Ensure(await users.CreateAsync(account, password));
            }
            var role = accounts.Count == 0 ? "Admin" : "Customer";
            if (!await users.IsInRoleAsync(account, role))
            {
                Ensure(await users.AddToRoleAsync(account, role));
            }
            accounts.Add(account);
        }

        var genres = new List<Genre>();
        foreach (var name in new[] { "Hành động", "Phiêu lưu", "Tình cảm" })
        {
            var genre = await db.Genres.FirstOrDefaultAsync(g => g.Name == name);
            if (genre == null)
            {
                genre = new Genre { Name = name };
                db.Genres.Add(genre);
            }
            genres.Add(genre);
        }
        await db.SaveChangesAsync();

        string[] titles = { "Hành trình bình minh", "Bí mật căn phòng cuối", "Chuyến tàu mùa hạ",
            "Thành phố ánh sáng", "Hẹn ngày trở lại", "Phía bên kia đại dương", "Dấu chân trên mây",
            "Những ngày rực rỡ", "Lời hứa cuối mùa", "Ngọn hải đăng xa" };
        string[] descriptions = {
            "Một nhóm bạn vượt qua những thử thách để tìm lại người đồng đội mất tích và khám phá ý nghĩa của lòng tin.",
            "Trở về căn nhà cũ, một cô gái phát hiện những lá thư dẫn đến bí mật mà gia đình đã giấu kín nhiều năm.",
            "Chuyến tàu cuối mùa hè đưa những người xa lạ đến với nhau và mở ra một khởi đầu mới.",
            "Một kỹ sư trẻ cùng đội cứu hộ chạy đua với thời gian để đưa ánh sáng trở lại thành phố.",
            "Hai người bạn gặp lại sau nhiều năm và quyết định hoàn thành lời hẹn từ thời thanh xuân.",
            "Hành trình vượt biển giúp một gia đình tìm lại sự gắn kết và niềm hy vọng vào ngày mai.",
            "Đoàn thám hiểm trẻ chinh phục đỉnh núi đầu tiên, vượt qua giới hạn của bản thân.",
            "Những thành viên của một ban nhạc nhỏ cùng chuẩn bị buổi biểu diễn quan trọng nhất cuộc đời.",
            "Giữa những đổi thay của làng quê, một người trẻ trở về để gìn giữ ước mơ của gia đình.",
            "Người gác hải đăng và một vị khách bất ngờ cùng tìm lời giải cho tín hiệu bí ẩn ngoài khơi."
        };
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
        var movies = new List<Movie>();
        for (var i = 0; i < titles.Length; i++)
        {
            var title = titles[i];
            var movie = await db.Movies.FirstOrDefaultAsync(m => m.Title == title);
            if (movie == null)
            {
                movie = new Movie
                {
                    Title = title,
                    Description = descriptions[i],
                    DurationMinutes = 90 + i * 2,
                    ReleaseDate = now.Date.AddDays(-1),
                    IsActive = i != 9,
                    PosterUrl = $"/images/posters/demo-{i + 1:00}.jpg",
                    Genres = new List<Genre> { genres[i % genres.Count] }
                };
                db.Movies.Add(movie);
            }
            movies.Add(movie);
        }

        var rooms = new List<Room>();
        foreach (var name in new[] { "Demo R1", "Demo R2", "Demo R3", "Demo R4" })
        {
            var room = await db.Rooms.Include(r => r.Seats).FirstOrDefaultAsync(r => r.Name == name);
            if (room == null)
            {
                room = new Room { Name = name, IsActive = name != "Demo R2" };
                if (name != "Demo R3")
                {
                    for (var row = 0; row < 3; row++)
                        for (var number = 1; number <= 5; number++)
                        {
                            room.Seats.Add(new Seat
                            {
                                RowLabel = ((char)('A' + row)).ToString(),
                                SeatNumber = number,
                                IsActive = name != "Demo R4"
                            });
                        }
                }
                db.Rooms.Add(room);
            }
            rooms.Add(room);
        }
        await db.SaveChangesAsync();

        // Chỉ bổ sung lịch mới; không dịch chuyển suất hoặc vé đã tồn tại.
        for (var i = 0; i < 8; i++)
        {
            var start = now.Date.AddDays(1 + i / 4).AddHours(9 + 3 * (i % 4));
            var movie = movies[i];
            if (!await db.Showtimes.AnyAsync(s => s.RoomId == rooms[0].Id && s.StartTime == start))
            {
                db.Showtimes.Add(new Showtime
                {
                    MovieId = movie.Id,
                    RoomId = rooms[0].Id,
                    StartTime = start,
                    EndTime = start.AddMinutes(movie.DurationMinutes),
                    BasePrice = 80000 + i * 5000,
                    IsActive = true
                });
            }
        }
        var closedStart = now.Date.AddDays(1).AddHours(21);
        if (!await db.Showtimes.AnyAsync(s => s.RoomId == rooms[0].Id && s.StartTime == closedStart))
        {
            db.Showtimes.Add(new Showtime
            {
                MovieId = movies[0].Id,
                RoomId = rooms[0].Id,
                StartTime = closedStart,
                EndTime = closedStart.AddMinutes(movies[0].DurationMinutes),
                BasePrice = 80000,
                IsActive = false
            });
        }
        await db.SaveChangesAsync();

        var firstStart = now.Date.AddDays(1).AddHours(9);
        var first = await db.Showtimes.FirstAsync(s => s.RoomId == rooms[0].Id && s.StartTime == firstStart);
        if (!await db.Bookings.AnyAsync(b => b.ShowtimeId == first.Id))
        {
            var seats = rooms[0].Seats.Where(s => s.IsActive).OrderBy(s => s.RowLabel).ThenBy(s => s.SeatNumber).Take(12).ToList();
            for (var i = 0; i < seats.Count; i++)
            {
                db.Bookings.Add(new Booking
                {
                    UserId = accounts[1 + i % 2].Id,
                    ShowtimeId = first.Id,
                    TotalAmount = first.BasePrice,
                    PaymentStatus = i % 3 == 0 ? BookingPaymentStatus.Paid : BookingPaymentStatus.Unpaid,
                    BookingSeats = new List<BookingSeat> { new() { ShowtimeId = first.Id,
                        SeatId = seats[i].Id, UnitPrice = first.BasePrice } }
                });
            }
            await db.SaveChangesAsync();
        }
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
