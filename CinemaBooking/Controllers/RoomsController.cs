using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaBooking.Models;
using CinemaBooking.Data;

namespace CinemaBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(ApplicationDbContext context, ILogger<RoomsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms
                .AsNoTracking()
                .Include(r => r.Seats)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return View(rooms);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .AsNoTracking()
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.Id == id.Value);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        public IActionResult Create()
        {
            return View(new Room());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,IsActive")] Room room)
        {
            if (!ModelState.IsValid)
            {
                return View(room);
            }

            try
            {
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is DbUpdateException or SqlException)
            {
                _logger.LogError(ex, "Không thể tạo phòng");
                ModelState.AddModelError(string.Empty,
                    "Không thể lưu phòng. Vui lòng kiểm tra và thử lại.");
            }

            return View(room);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,IsActive")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(room);
            }

            var existingRoom = await _context.Rooms.FindAsync(id);

            if (existingRoom == null)
            {
                return NotFound();
            }

            existingRoom.Name = room.Name;
            existingRoom.IsActive = room.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Xung đột khi sửa phòng {RoomId}", room.Id);
                ModelState.AddModelError(string.Empty,
                    "Phòng đã được thay đổi hoặc xóa. Vui lòng tải lại trang.");
            }
            catch (Exception ex) when (ex is DbUpdateException or SqlException)
            {
                _logger.LogError(ex, "Không thể sửa phòng {RoomId}", room.Id);
                ModelState.AddModelError(string.Empty,
                    "Không thể lưu phòng. Vui lòng kiểm tra và thử lại.");
            }

            return View(room);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            var hasSeats = await _context.Seats
                .AnyAsync(s => s.RoomId == id);

            if (hasSeats)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Phòng này đã có ghế. Bạn có thể tắt trạng thái "
                    + "hoạt động thay vì xóa phòng.");

                return View("Delete", room);
            }

            try
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is DbUpdateException or SqlException)
            {
                _logger.LogError(ex, "Không thể lưu thay đổi dữ liệu");
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể xóa phòng. Phòng có thể đang được "
                    + "dữ liệu khác tham chiếu. Vui lòng tải lại "
                    + "trang và kiểm tra.");

                return View("Delete", room);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(r => r.Id == id);
        }
    }
}
