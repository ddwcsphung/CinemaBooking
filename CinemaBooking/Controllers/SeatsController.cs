using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers;

[Authorize(Roles = "Admin")]
public class SeatsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SeatsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Seats?roomId=1
    public async Task<IActionResult> Index(int roomId)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .Include(r => r.Seats)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

    // GET: Seats/Generate?roomId=1
    public async Task<IActionResult> Generate(int roomId)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
        {
            return NotFound();
        }

        if (await _context.Seats.AnyAsync(s => s.RoomId == roomId))
        {
            TempData["SeatMessage"] =
                "Phòng đã có ghế nên không thể tạo hàng loạt lần nữa.";

            return RedirectToAction(nameof(Index), new { roomId });
        }

        ViewBag.RoomName = room.Name;

        return View(new GenerateSeatsViewModel
        {
            RoomId = room.Id
        });
    }

    // POST: Seats/Generate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(
        GenerateSeatsViewModel model)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == model.RoomId);

        if (room == null)
        {
            return NotFound();
        }

        ViewBag.RoomName = room.Name;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _context.Seats.AnyAsync(
            s => s.RoomId == model.RoomId))
        {
            ModelState.AddModelError(
                string.Empty,
                "Phòng đã có ghế. Không thể tạo hàng loạt lần nữa.");

            return View(model);
        }

        var seats = new List<Seat>();

        for (int row = 0; row < model.RowCount; row++)
        {
            string rowLabel = ((char)('A' + row)).ToString();

            for (int number = 1; number <= model.SeatsPerRow; number++)
            {
                seats.Add(new Seat
                {
                    RoomId = model.RoomId,
                    RowLabel = rowLabel,
                    SeatNumber = number,
                    IsActive = true
                });
            }
        }

        _context.Seats.AddRange(seats);

        try
        {
            // Lưu tất cả ghế trong một lần.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không thể tạo ghế. Dữ liệu phòng hoặc ghế có thể "
                + "đã thay đổi. Hãy quay lại danh sách ghế để kiểm tra.");

            return View(model);
        }

        TempData["SeatMessage"] =
            $"Đã tạo thành công {seats.Count} ghế cho {room.Name}.";

        return RedirectToAction(
            nameof(Index),
            new { roomId = model.RoomId });
    }
}