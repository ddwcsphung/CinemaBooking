using System.Data;
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<BookingsController> _logger;

    private static DateTime CinemaNow =>
        DateTimeOffset.UtcNow
            .ToOffset(TimeSpan.FromHours(7))
            .DateTime;

    public BookingsController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        ILogger<BookingsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: /Bookings/Create?showtimeId=1
    [HttpGet]
    public async Task<IActionResult> Create(int showtimeId)
    {
        var now = CinemaNow;

        var showtime = await _context.Showtimes
            .AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s =>
                s.Id == showtimeId &&
                s.IsActive &&
                s.Movie.IsActive &&
                s.Room.IsActive &&
                s.StartTime > now);

        if (showtime == null)
        {
            return NotFound("Suất chiếu không còn mở bán.");
        }

        var seats = await _context.Seats
            .AsNoTracking()
            .Where(s => s.RoomId == showtime.RoomId)
            .OrderBy(s => s.RowLabel)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();

        var bookedIds = await _context.BookingSeats
            .AsNoTracking()
            .Where(bs => bs.ShowtimeId == showtimeId)
            .Select(bs => bs.SeatId)
            .ToListAsync();

        var bookedSet = bookedIds.ToHashSet();

        var model = new BookingCreateViewModel
        {
            ShowtimeId = showtime.Id,
            MovieTitle = showtime.Movie.Title,
            RoomName = showtime.Room.Name,
            StartTime = showtime.StartTime,
            BasePrice = showtime.BasePrice,

            Seats = seats.Select(s => new BookingSeatViewModel
            {
                SeatId = s.Id,
                RowLabel = s.RowLabel,
                SeatNumber = s.SeatNumber,
                IsActive = s.IsActive,
                IsBooked = bookedSet.Contains(s.Id)
            }).ToList()
        };

        return View(model);
    }

    // POST: /Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        BookingRequestViewModel request)
    {
        if (!ModelState.IsValid)
        {
            return BookingError(
                request.ShowtimeId,
                "Vui lòng chọn từ 1 đến 10 ghế hợp lệ.");
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var seatIds = request.SelectedSeatIds
            .Distinct()
            .ToArray();

        if (seatIds.Length != request.SelectedSeatIds.Count)
        {
            return BookingError(
                request.ShowtimeId,
                "Danh sách ghế bị trùng. Vui lòng chọn lại.");
        }

        try
        {
            // Kiểm tra và lưu trong cùng một giao dịch.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s =>
                    s.Id == request.ShowtimeId);

            if (showtime == null ||
                !showtime.IsActive ||
                !showtime.Movie.IsActive ||
                !showtime.Room.IsActive ||
                showtime.StartTime <= CinemaNow)
            {
                return BookingError(
                    request.ShowtimeId,
                    "Suất chiếu không còn đủ điều kiện đặt vé.");
            }

            // Ghế phải thuộc đúng phòng và đang sử dụng.
            var validSeatCount = await _context.Seats
                .CountAsync(s =>
                    seatIds.Contains(s.Id) &&
                    s.RoomId == showtime.RoomId &&
                    s.IsActive);

            if (validSeatCount != seatIds.Length)
            {
                return BookingError(
                    request.ShowtimeId,
                    "Có ghế không hợp lệ hoặc đã ngừng sử dụng.");
            }

            var hasBookedSeat = await _context.BookingSeats
                .AnyAsync(bs =>
                    bs.ShowtimeId == showtime.Id &&
                    seatIds.Contains(bs.SeatId));

            if (hasBookedSeat)
            {
                return BookingError(
                    request.ShowtimeId,
                    "Có ghế vừa được người khác đặt. Vui lòng chọn lại.");
            }

            // Kiểm tra lại thời gian sau các truy vấn.
            if (showtime.StartTime <= CinemaNow)
            {
                return BookingError(
                    request.ShowtimeId,
                    "Suất chiếu đã bắt đầu.");
            }

            var booking = new Booking
            {
                UserId = user.Id,
                ShowtimeId = showtime.Id,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                PaymentStatus = BookingPaymentStatus.Unpaid,

                // Giá lấy từ database, không lấy từ trình duyệt.
                TotalAmount = showtime.BasePrice * seatIds.Length,

                BookingSeats = seatIds.Select(seatId =>
                    new BookingSeat
                    {
                        ShowtimeId = showtime.Id,
                        SeatId = seatId,
                        UnitPrice = showtime.BasePrice
                    }).ToList()
            };

            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Đơn đã được lưu thành công.
            // Lỗi dọn session không được biến thành thông báo đặt vé thất bại.
            try
            {
                await HttpContext.Session.LoadAsync();

                var selectionKey =
                    $"SeatSelection:{user.Id}:{booking.ShowtimeId}";

                HttpContext.Session.Remove(selectionKey);
                await HttpContext.Session.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Đã lưu đơn {BookingId} nhưng chưa xóa được lựa chọn tạm.",
                    booking.Id);
            }

            TempData["BookingSuccess"] = "Đặt vé thành công.";

            return RedirectToAction(
                nameof(Details),
                new { id = booking.Id });
        }
        catch (DbUpdateException ex)
            when (ex.GetBaseException() is SqlException sql &&
                  (sql.Number == 2601 || sql.Number == 2627))
        {
            // Database từ chối dữ liệu trùng.
            return BookingError(
                request.ShowtimeId,
                "Dữ liệu đặt vé bị trùng hoặc ghế vừa được đặt. "
                + "Vui lòng tải lại và chọn ghế.");
        }
        catch (Exception ex)
            when (ex.GetBaseException() is SqlException sql &&
                  sql.Number == 1205)
        {
            // Hai giao dịch cạnh tranh: SQL Server hủy một giao dịch.
            return BookingError(
                request.ShowtimeId,
                "Có người đang đặt vé cùng lúc. "
                + "Vui lòng kiểm tra ghế và thử lại.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể lưu đơn đặt vé.");

            return BookingError(
                request.ShowtimeId,
                "Không thể lưu đơn. Vui lòng tải lại trang và thử lại.");
        }
    }

    // GET: /Bookings
    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        // Chỉ lấy các đơn của người đang đăng nhập.
        var query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId);

        var totalItems = await query.CountAsync();

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(totalItems / (double)pageSize));

        page = Math.Clamp(page, 1, totalPages);

        var bookings = await query
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Room)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ThenByDescending(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(bookings);
    }

    // GET: /Bookings/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Room)
            .Include(b => b.BookingSeats)
                .ThenInclude(bs => bs.Seat)
            .FirstOrDefaultAsync(b =>
                b.Id == id &&
                b.UserId == userId);

        // Chỉ chủ đơn được xem.
        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    private IActionResult BookingError(int showtimeId, string message)
    {
        TempData["BookingError"] = message;

        return RedirectToAction(
            nameof(Create),
            new { showtimeId });
    }
}