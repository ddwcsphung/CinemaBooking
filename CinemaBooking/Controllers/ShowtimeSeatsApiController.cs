using CinemaBooking.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers;

[ApiController]
[Authorize]
[Route("api/showtimes")]
public class ShowtimeSeatsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ShowtimeSeatsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{showtimeId:int}/seats")]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetSeats(
        int showtimeId,
        CancellationToken cancellationToken)
    {
        if (showtimeId <= 0)
        {
            return BadRequest(new
            {
                message = "Mã suất chiếu không hợp lệ."
            });
        }

        var now = DateTimeOffset.UtcNow
            .ToOffset(TimeSpan.FromHours(7))
            .DateTime;

        var showtime = await _context.Showtimes
            .AsNoTracking()
            .Where(s =>
                s.Id == showtimeId &&
                s.IsActive &&
                s.Movie.IsActive &&
                s.Room.IsActive &&
                s.StartTime > now)
            .Select(s => new
            {
                s.Id,
                s.RoomId,
                s.BasePrice
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (showtime == null)
        {
            return NotFound(new
            {
                message = "Suất chiếu không còn mở bán."
            });
        }

        var seats = await _context.Seats
            .AsNoTracking()
            .Where(s => s.RoomId == showtime.RoomId)
            .OrderBy(s => s.RowLabel)
            .ThenBy(s => s.SeatNumber)
            .Select(s => new
            {
                seatId = s.Id,
                rowLabel = s.RowLabel,
                seatNumber = s.SeatNumber,
                isActive = s.IsActive,

                isBooked = _context.BookingSeats.Any(bs =>
                    bs.ShowtimeId == showtimeId &&
                    bs.SeatId == s.Id)
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            showtimeId = showtime.Id,
            basePrice = showtime.BasePrice,
            seats
        });
    }
}
