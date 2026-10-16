using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaBooking.Models;
using CinemaBooking.Data;


[Authorize(Roles = "Admin")]
public class GenresController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GenresController> _logger;

    public GenresController(ApplicationDbContext context, ILogger<GenresController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: GENRES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Genres.ToListAsync());
    }

    // GET: Genres/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var genre = await _context.Genres
            .AsNoTracking()
            .Include(g => g.Movies)
            .FirstOrDefaultAsync(g => g.Id == id.Value);

        if (genre == null)
        {
            return NotFound();
        }

        return View(genre);
    }

    // GET: GENRES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: GENRES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name")] Genre genre)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(genre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is DbUpdateException or SqlException)
            {
                _logger.LogError(ex, "Không thể tạo thể loại");
                ModelState.AddModelError(string.Empty,
                    "Không thể lưu thể loại. Vui lòng kiểm tra và thử lại.");
            }
        }
        return View(genre);
    }

    // GET: GENRES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var genre = await _context.Genres.FindAsync(id);
        if (genre == null)
        {
            return NotFound();
        }
        return View(genre);
    }

    // POST: GENRES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name")] Genre genre)
    {
        if (id != genre.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(genre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Xung đột khi sửa thể loại {GenreId}", genre.Id);
                ModelState.AddModelError(string.Empty,
                    "Thể loại đã được thay đổi hoặc xóa. Vui lòng tải lại trang.");
            }
            catch (Exception ex) when (ex is DbUpdateException or SqlException)
            {
                _logger.LogError(ex, "Không thể sửa thể loại {GenreId}", genre.Id);
                ModelState.AddModelError(string.Empty,
                    "Không thể lưu thể loại. Vui lòng kiểm tra và thử lại.");
            }
        }
        return View(genre);
    }

    // GET: GENRES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var genre = await _context.Genres
            .FirstOrDefaultAsync(m => m.Id == id);
        if (genre == null)
        {
            return NotFound();
        }

        return View(genre);
    }

    // POST: Genres/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Genre? genre = null;

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            genre = await _context.Genres
                .FirstOrDefaultAsync(g => g.Id == id.Value);

            if (genre == null)
            {
                return NotFound();
            }

            // Kiểm tra cả phim đang hiển thị và phim đã ẩn.
            var isUsed = await _context.Movies
                .AnyAsync(m => m.Genres.Any(g => g.Id == id.Value));

            if (isUsed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể xóa thể loại đang được phim sử dụng. "
                    + "Hãy chỉnh lại thể loại của các phim liên quan trước.");

                return View("Delete", genre);
            }

            _context.Genres.Remove(genre);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
            when (ex.GetBaseException() is SqlException sql &&
                  sql.Number == 1205)
        {
            _logger.LogWarning(ex, "Xung đột giao dịch dữ liệu");
            ModelState.AddModelError(
                string.Empty,
                "Có người đang cập nhật dữ liệu cùng lúc. "
                + "Vui lòng tải lại trang và thử lại.");
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException)
        {
            _logger.LogError(ex, "Không thể lưu thay đổi dữ liệu");
            ModelState.AddModelError(
                string.Empty,
                "Không thể xóa thể loại vì dữ liệu đã thay đổi "
                + "hoặc đang được sử dụng.");
        }

        // Trường hợp lỗi xảy ra trước khi lấy được thể loại.
        if (genre == null)
        {
            genre = await _context.Genres
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id.Value);

            if (genre == null)
            {
                return NotFound();
            }
        }

        return View("Delete", genre);
    }

    private bool GenreExists(int? id)
    {
        return _context.Genres.Any(e => e.Id == id);
    }
}
