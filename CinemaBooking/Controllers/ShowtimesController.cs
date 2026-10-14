using System.Data;
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers;

[Authorize(Roles = "Admin")]
public class ShowtimesController : Controller
{
    private readonly ApplicationDbContext _context;

    private const int CleaningMinutes = 15;

    public ShowtimesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Quy ước lịch chiếu của rạp dùng giờ Việt Nam (UTC+7).
    private static DateTime CinemaNow =>
        DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;

    public async Task<IActionResult> Index()
    {
        var showtimes = await _context.Showtimes
            .AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        return View(showtimes);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ShowtimeCreateViewModel();

        await LoadOptionsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ShowtimeCreateViewModel model)
    {
        if (model.StartTime.HasValue &&
            model.StartTime.Value <= CinemaNow)
        {
            ModelState.AddModelError(
                nameof(model.StartTime),
                "Giờ bắt đầu phải nằm trong tương lai.");
        }

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(model);
            return View(model);
        }

        // Giữ việc kiểm tra lịch và lưu suất trong cùng giao dịch.
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m =>
                    m.Id == model.MovieId && m.IsActive);

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r =>
                    r.Id == model.RoomId && r.IsActive);

            if (movie == null)
            {
                ModelState.AddModelError(
                    nameof(model.MovieId),
                    "Phim không tồn tại hoặc đang bị ẩn.");
            }

            if (room == null)
            {
                ModelState.AddModelError(
                    nameof(model.RoomId),
                    "Phòng không tồn tại hoặc đã ngừng hoạt động.");
            }
            else if (!await _context.Seats.AnyAsync(s =>
                s.RoomId == room.Id && s.IsActive))
            {
                ModelState.AddModelError(
                    nameof(model.RoomId),
                    "Phòng chưa có ghế đang sử dụng.");
            }

            if (movie != null)
            {
                var startTime = model.StartTime!.Value;
                var endTime =
                    startTime.AddMinutes(movie.DurationMinutes);

                var newRoomAvailableTime =
                    endTime.AddMinutes(CleaningMinutes);

                // Kiểm tra cả suất tạm đóng bán vì vẫn chiếm lịch phòng.
                var overlaps = await _context.Showtimes.AnyAsync(s =>
                    s.RoomId == model.RoomId &&
                    s.StartTime < newRoomAvailableTime &&
                    s.EndTime.AddMinutes(CleaningMinutes) > startTime);

                if (overlaps)
                {
                    ModelState.AddModelError(
                        nameof(model.StartTime),
                        "Phòng đã có lịch trong khoảng thời gian này. "
                        + "Cần chừa 15 phút dọn phòng giữa hai suất.");
                }

                if (ModelState.IsValid)
                {
                    var showtime = new Showtime
                    {
                        MovieId = movie.Id,
                        RoomId = model.RoomId,
                        StartTime = startTime,
                        EndTime = endTime,
                        BasePrice = model.BasePrice!.Value,
                        IsActive = model.IsActive
                    };

                    _context.Showtimes.Add(showtime);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["ShowtimeMessage"] =
                        "Đã tạo suất chiếu thành công.";

                    return RedirectToAction(nameof(Index));
                }
            }

            await transaction.RollbackAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Không thể lưu suất chiếu. Dữ liệu có thể đã thay đổi. "
                + "Vui lòng kiểm tra và thử lại.");
        }
        catch (SqlException ex) when (ex.Number == 1205)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Có người khác đang cập nhật lịch. Vui lòng thử lưu lại.");
        }

        await LoadOptionsAsync(model);

        return View(model);
    }

    // POST: Showtimes/ToggleSale/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSale(int id)
    {
        var showtime = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (showtime == null)
        {
            return NotFound();
        }

        // Khi chuyển từ đóng sang mở bán, kiểm tra điều kiện.
        if (!showtime.IsActive)
        {
            if (showtime.StartTime <= CinemaNow)
            {
                TempData["ShowtimeError"] =
                    "Không thể mở bán suất chiếu đã bắt đầu.";

                return RedirectToAction(nameof(Index));
            }

            if (!showtime.Movie.IsActive || !showtime.Room.IsActive)
            {
                TempData["ShowtimeError"] =
                    "Không thể mở bán vì phim đang bị ẩn "
                    + "hoặc phòng đã ngừng hoạt động.";

                return RedirectToAction(nameof(Index));
            }

            var hasActiveSeats = await _context.Seats.AnyAsync(s =>
                s.RoomId == showtime.RoomId && s.IsActive);

            if (!hasActiveSeats)
            {
                TempData["ShowtimeError"] =
                    "Không thể mở bán vì phòng không có ghế đang sử dụng.";

                return RedirectToAction(nameof(Index));
            }
        }

        showtime.IsActive = !showtime.IsActive;

        await _context.SaveChangesAsync();

        TempData["ShowtimeMessage"] = showtime.IsActive
            ? "Đã mở bán suất chiếu."
            : "Đã đóng bán suất chiếu.";

        return RedirectToAction(nameof(Index));
    }

    // POST: Showtimes/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var showtime = await _context.Showtimes.FindAsync(id);

        if (showtime == null)
        {
            return NotFound();
        }

        // Giữ lại lịch sử các suất đã bắt đầu.
        if (showtime.StartTime <= CinemaNow)
        {
            TempData["ShowtimeError"] =
                "Không xóa suất chiếu đã bắt đầu. "
                + "Bạn có thể đóng bán suất này.";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Showtimes.Remove(showtime);
            await _context.SaveChangesAsync();

            TempData["ShowtimeMessage"] = "Đã xóa suất chiếu.";
        }
        catch (DbUpdateException)
        {
            TempData["ShowtimeError"] =
                "Không thể xóa suất chiếu vì có lỗi lưu dữ liệu "
                + "hoặc dữ liệu liên quan. Hãy tải lại trang và kiểm tra.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadOptionsAsync(
        ShowtimeCreateViewModel model)
    {
        var movies = await _context.Movies
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.Title)
            .ToListAsync();

        model.MovieOptions = movies.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = $"{m.Title} ({m.DurationMinutes} phút)"
        }).ToList();

        var rooms = await _context.Rooms
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();

        model.RoomOptions = rooms.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = $"{r.Name} — mã {r.Id}"
        }).ToList();
    }
}