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
        if (model.StartTime.HasValue)
        {
            var startTime = model.StartTime.Value;


            if (startTime <= CinemaNow)
            {
                ModelState.AddModelError(
                    nameof(model.StartTime),
                    "Giờ bắt đầu phải nằm trong tương lai.");
            }

            // Chừa khoảng thời gian để cộng thời lượng phim
            // và 15 phút dọn phòng mà không vượt giới hạn DateTime.
            if (startTime >= new DateTime(9999, 12, 31))
            {
                ModelState.AddModelError(
                    nameof(model.StartTime),
                    "Ngày chiếu nằm ngoài phạm vi được hỗ trợ.");
            }
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

                if (startTime.Date < movie.ReleaseDate.Date)
                {
                    ModelState.AddModelError(
                        nameof(model.StartTime),
                        $"Ngày chiếu phải từ ngày khởi chiếu " +
                        $"{movie.ReleaseDate:dd/MM/yyyy} trở đi.");
                }

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

                if (startTime <= CinemaNow)
                {
                    ModelState.AddModelError(
                        nameof(model.StartTime),
                        "Giờ bắt đầu đã qua trong lúc xử lý. " +
                        "Vui lòng chọn giờ khác.");
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

        var hasBookings = await _context.Bookings
    .AnyAsync(b => b.ShowtimeId == id);

        if (hasBookings)
        {
            TempData["ShowtimeError"] =
                "Không thể xóa suất chiếu đã có đơn đặt vé, " +
                "kể cả đơn chưa thanh toán. " +
                "Bạn có thể đóng bán để ngừng nhận thêm đơn.";

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

    // GET: /Showtimes/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var showtime = await _context.Showtimes
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (showtime == null)
        {
            return NotFound();
        }

        if (showtime.StartTime <= CinemaNow ||
            await _context.Bookings.AnyAsync(b => b.ShowtimeId == id))
        {
            TempData["ShowtimeError"] =
                "Chỉ được sửa suất chưa bắt đầu và chưa có đơn đặt vé.";

            return RedirectToAction(nameof(Index));
        }

        var model = new ShowtimeCreateViewModel
        {
            MovieId = showtime.MovieId,
            RoomId = showtime.RoomId,
            StartTime = showtime.StartTime,
            BasePrice = showtime.BasePrice,
            IsActive = showtime.IsActive
        };

        ViewBag.ShowtimeId = id;
        await LoadOptionsAsync(model);

        return View(model);
    }

    // POST: /Showtimes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ShowtimeCreateViewModel model)
    {
        ViewBag.ShowtimeId = id;

        if (model.StartTime.HasValue)
        {
            if (model.StartTime.Value <= CinemaNow)
            {
                ModelState.AddModelError(
                    nameof(model.StartTime),
                    "Giờ bắt đầu phải nằm trong tương lai.");
            }

            if (model.StartTime.Value >= new DateTime(9999, 12, 31))
            {
                ModelState.AddModelError(
                    nameof(model.StartTime),
                    "Ngày chiếu nằm ngoài phạm vi được hỗ trợ.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(model);
            return View(model);
        }

        try
        {
            // Kiểm tra điều kiện và lưu trong cùng giao dịch.
            // Đặt vé đồng thời không được làm thay đổi vé đã tạo.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var showtime = await _context.Showtimes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
            {
                return NotFound();
            }

            // Phải kiểm tra lại khi lưu vì có thể vừa phát sinh đơn.
            if (showtime.StartTime <= CinemaNow ||
                await _context.Bookings.AnyAsync(b => b.ShowtimeId == id))
            {
                TempData["ShowtimeError"] =
                    "Không thể sửa vì suất đã bắt đầu hoặc đã có đơn đặt vé.";

                return RedirectToAction(nameof(Index));
            }

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

                if (startTime.Date < movie.ReleaseDate.Date)
                {
                    ModelState.AddModelError(
                        nameof(model.StartTime),
                        $"Ngày chiếu phải từ ngày khởi chiếu " +
                        $"{movie.ReleaseDate:dd/MM/yyyy} trở đi.");
                }

                var endTime =
                    startTime.AddMinutes(movie.DurationMinutes);

                var newRoomAvailableTime =
                    endTime.AddMinutes(CleaningMinutes);

                var overlaps = await _context.Showtimes.AnyAsync(s =>
                    s.Id != id &&
                    s.RoomId == model.RoomId &&
                    s.StartTime < newRoomAvailableTime &&
                    s.EndTime.AddMinutes(CleaningMinutes) > startTime);

                if (overlaps)
                {
                    ModelState.AddModelError(
                        nameof(model.StartTime),
                        "Phòng đã có lịch trong khoảng thời gian này. " +
                        "Cần chừa 15 phút dọn phòng giữa hai suất.");
                }

                if (startTime <= CinemaNow ||
                    showtime.StartTime <= CinemaNow)
                {
                    ModelState.AddModelError(
                        nameof(model.StartTime),
                        "Giờ bắt đầu đã qua trong lúc xử lý. " +
                        "Vui lòng tải lại trang.");
                }

                if (ModelState.IsValid)
                {
                    showtime.MovieId = movie.Id;
                    showtime.RoomId = model.RoomId;
                    showtime.StartTime = startTime;
                    showtime.EndTime = endTime;
                    showtime.BasePrice = model.BasePrice!.Value;
                    showtime.IsActive = model.IsActive;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["ShowtimeMessage"] =
                        "Đã cập nhật suất chiếu thành công.";

                    return RedirectToAction(nameof(Index));
                }
            }

            // Khi ra khỏi khối try, giao dịch chưa commit sẽ được hủy.
        }
        catch (Exception ex)
            when (ex.GetBaseException() is SqlException sql &&
                  sql.Number == 1205)
        {
            ModelState.AddModelError(
                string.Empty,
                "Có người đang đặt vé hoặc cập nhật lịch cùng lúc. " +
                "Vui lòng tải lại trang và thử lại.");
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không thể cập nhật suất chiếu. " +
                "Dữ liệu có thể đã thay đổi, vui lòng tải lại trang.");
        }

        await LoadOptionsAsync(model);
        return View(model);
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