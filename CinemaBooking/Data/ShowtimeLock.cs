using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Data;

internal static class ShowtimeLock
{
    public static Task<int> AcquireAsync(ApplicationDbContext context, int showtimeId)
    {
        // SQL Server: cùng suất chiếu dùng chung thứ tự khóa trước khi đọc ghế.
        return context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT Id FROM dbo.Showtimes WITH (UPDLOCK, HOLDLOCK) WHERE Id = {showtimeId}");
    }
}
