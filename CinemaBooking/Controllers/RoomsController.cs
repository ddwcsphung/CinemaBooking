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

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync();

            return View(rooms);
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Rooms/Create
        public IActionResult Create()
        {
            return View(new Room());
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,IsActive")] Room room)
        {
            if (!ModelState.IsValid)
            {
                return View(room);
            }

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Rooms/Edit/5
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

        // POST: Rooms/Edit/5
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
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(room.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Rooms/Delete/5
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

        // POST: Rooms/Delete/5
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
            catch (DbUpdateException)
            {
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