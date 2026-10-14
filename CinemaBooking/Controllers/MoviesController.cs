using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .Include(m => m.Genres)
                .OrderBy(m => m.Title)
                .ToListAsync();

            return View(movies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .AsNoTracking()
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public async Task<IActionResult> Create()
        {
            await LoadGenresAsync(Array.Empty<int>());

            return View(new Movie());
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,DurationMinutes,ReleaseDate,PosterUrl,IsActive")]
            Movie movie,
            int[] selectedGenreIds)
        {
            var genres = await ValidateGenresAsync(selectedGenreIds);

            if (!ModelState.IsValid)
            {
                await LoadGenresAsync(selectedGenreIds);
                return View(movie);
            }

            movie.Genres = genres;

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var selectedIds = movie.Genres
                .Select(g => g.Id)
                .ToArray();

            await LoadGenresAsync(selectedIds);

            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Title,Description,DurationMinutes,ReleaseDate,PosterUrl,IsActive")]
            Movie movie,
            int[] selectedGenreIds)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            var existingMovie = await _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMovie == null)
            {
                return NotFound();
            }

            var genres = await ValidateGenresAsync(selectedGenreIds);

            if (!ModelState.IsValid)
            {
                await LoadGenresAsync(selectedGenreIds);
                return View(movie);
            }

            // Cập nhật thông tin phim.
            existingMovie.Title = movie.Title;
            existingMovie.Description = movie.Description;
            existingMovie.DurationMinutes = movie.DurationMinutes;
            existingMovie.ReleaseDate = movie.ReleaseDate;
            existingMovie.PosterUrl = movie.PosterUrl;
            existingMovie.IsActive = movie.IsActive;

            // Gỡ các liên kết thể loại cũ, không xóa thể loại.
            existingMovie.Genres.Clear();

            // Gán các thể loại được chọn.
            foreach (var genre in genres)
            {
                existingMovie.Genres.Add(genre);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await MovieExistsAsync(movie.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .AsNoTracking()
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Nạp danh sách lựa chọn và đánh dấu các thể loại đã chọn.
        private async Task LoadGenresAsync(int[] selectedGenreIds)
        {
            var genres = await _context.Genres
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .ToListAsync();

            ViewBag.GenreOptions = new MultiSelectList(
                genres,
                "Id",
                "Name",
                selectedGenreIds);
        }

        // Kiểm tra các mã thể loại gửi từ biểu mẫu.
        private async Task<List<Genre>> ValidateGenresAsync(
            int[] selectedGenreIds)
        {
            var ids = selectedGenreIds.Distinct().ToArray();

            var genres = await _context.Genres
                .Where(g => ids.Contains(g.Id))
                .ToListAsync();

            if (ids.Length == 0)
            {
                ModelState.AddModelError(
                    "selectedGenreIds",
                    "Vui lòng chọn ít nhất một thể loại.");
            }
            else if (genres.Count != ids.Length)
            {
                ModelState.AddModelError(
                    "selectedGenreIds",
                    "Có thể loại không còn tồn tại. Vui lòng chọn lại.");
            }

            return genres;
        }

        private Task<bool> MovieExistsAsync(int id)
        {
            return _context.Movies.AnyAsync(m => m.Id == id);
        }
    }
}