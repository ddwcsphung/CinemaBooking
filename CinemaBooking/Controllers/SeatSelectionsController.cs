using System.Security.Claims;
using System.Text.Json;
using CinemaBooking.Data;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers;

[ApiController]
[Authorize]
[Route("api/showtimes/{showtimeId:int}/selection")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class SeatSelectionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SeatSelectionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static DateTime CinemaNow =>
        DateTimeOffset.UtcNow
            .ToOffset(TimeSpan.FromHours(7))
            .DateTime;

    // GET: /api/showtimes/4/selection
    [HttpGet]
    public async Task<IActionResult> GetSelection(
        int showtimeId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (showtimeId <= 0)
        {
            return BadRequest(new
            {
                message = "Mã suất chiếu không hợp lệ."
            });
        }

        var roomId = await GetAvailableRoomIdAsync(
            showtimeId, cancellationToken);

        if (roomId == null)
        {
            return NotFound(new
            {
                message = "Suất chiếu không còn mở bán."
            });
        }

        await HttpContext.Session.LoadAsync(cancellationToken);

        var key = GetSessionKey(userId, showtimeId);
        var savedJson = HttpContext.Session.GetString(key);

        var savedIds = string.IsNullOrEmpty(savedJson)
            ? new List<int>()
            : JsonSerializer.Deserialize<List<int>>(savedJson)
                ?? new List<int>();

        // Chỉ trả lại ghế vẫn còn trống và đang sử dụng.
        var validIds = await GetAvailableSeatIdsAsync(
            showtimeId,
            roomId.Value,
            savedIds,
            cancellationToken);

        return Ok(new
        {
            showtimeId,
            seatIds = validIds,
            removedSeatIds = savedIds.Except(validIds).ToArray()
        });
    }

    // POST: /api/showtimes/4/selection
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSelection(
        int showtimeId,
        [FromBody] SeatSelectionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (showtimeId <= 0)
        {
            return BadRequest(new
            {
                message = "Mã suất chiếu không hợp lệ."
            });
        }

        // ApiController kiểm tra Required và MaxLength trước action.
        var requestedIds = request.SeatIds.Distinct().ToList();

        if (requestedIds.Any(id => id <= 0) ||
            requestedIds.Count != request.SeatIds.Count)
        {
            return BadRequest(new
            {
                message = "Danh sách ghế không hợp lệ hoặc bị trùng."
            });
        }

        var roomId = await GetAvailableRoomIdAsync(
            showtimeId, cancellationToken);

        if (roomId == null)
        {
            return NotFound(new
            {
                message = "Suất chiếu không còn mở bán."
            });
        }

        var validIds = await GetAvailableSeatIdsAsync(
            showtimeId,
            roomId.Value,
            requestedIds,
            cancellationToken);

        await HttpContext.Session.LoadAsync(cancellationToken);

        var key = GetSessionKey(userId, showtimeId);

        if (validIds.Count == 0)
        {
            HttpContext.Session.Remove(key);
        }
        else
        {
            HttpContext.Session.SetString(
                key,
                JsonSerializer.Serialize(validIds));
        }

        await HttpContext.Session.CommitAsync(cancellationToken);

        return Ok(new
        {
            showtimeId,
            seatIds = validIds,
            removedSeatIds = requestedIds.Except(validIds).ToArray()
        });
    }

    private static string GetSessionKey(string userId, int showtimeId)
    {
        return $"SeatSelection:{userId}:{showtimeId}";
    }

    private async Task<int?> GetAvailableRoomIdAsync(
        int showtimeId,
        CancellationToken cancellationToken)
    {
        var now = CinemaNow;

        return await _context.Showtimes
            .AsNoTracking()
            .Where(s =>
                s.Id == showtimeId &&
                s.IsActive &&
                s.Movie.IsActive &&
                s.Room.IsActive &&
                s.StartTime > now)
            .Select(s => (int?)s.RoomId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<List<int>> GetAvailableSeatIdsAsync(
        int showtimeId,
        int roomId,
        List<int> seatIds,
        CancellationToken cancellationToken)
    {
        if (seatIds.Count == 0)
        {
            return new List<int>();
        }

        return await _context.Seats
            .AsNoTracking()
            .Where(s =>
                seatIds.Contains(s.Id) &&
                s.RoomId == roomId &&
                s.IsActive &&
                !_context.BookingSeats.Any(bs =>
                    bs.ShowtimeId == showtimeId &&
                    bs.SeatId == s.Id))
            .OrderBy(s => s.RowLabel)
            .ThenBy(s => s.SeatNumber)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);
    }
}