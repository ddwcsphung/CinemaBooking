using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminBookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /AdminBookings
    public async Task<IActionResult> Index(
        string? keyword,
        int page = 1)
    {
        const int pageSize = 10;

        keyword = keyword?.Trim();

        var query = _context.Bookings
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(b =>
                b.BookingCode.Contains(keyword));
        }

        var totalItems = await query.CountAsync();

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(totalItems / (double)pageSize));

        page = Math.Clamp(page, 1, totalPages);

        var bookings = await query
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Room)
            .Include(b => b.BookingSeats)
                .ThenInclude(bs => bs.Seat)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ThenByDescending(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Keyword = keyword;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(bookings);
    }

    // POST: /AdminBookings/ConfirmPayment/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(
        int id,
        string? keyword,
        int page = 1)
    {
        // Chỉ cập nhật khi đơn vẫn chưa thanh toán.
        // Việc kiểm tra và cập nhật nằm trong cùng một câu SQL.
        var affectedRows = await _context.Bookings
            .Where(b =>
                b.Id == id &&
                b.PaymentStatus == BookingPaymentStatus.Unpaid)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    b => b.PaymentStatus,
                    BookingPaymentStatus.Paid));

        if (affectedRows == 1)
        {
            TempData["AdminBookingSuccess"] =
                "Đã xác nhận thanh toán cho đơn đặt vé.";
        }
        else
        {
            var exists = await _context.Bookings
                .AnyAsync(b => b.Id == id);

            if (!exists)
            {
                return NotFound();
            }

            TempData["AdminBookingInfo"] =
                "Đơn này đã được thanh toán trước đó. "
                + "Không cần xác nhận lại.";
        }

        return RedirectToAction(
            nameof(Index),
            new { keyword, page });
    }
}