using System.Data;
using Microsoft.Data.SqlClient;
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
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(ApplicationDbContext context, ILogger<MoviesController> logger)
        {
            _context = context;
            _logger = logger;
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

            try
            {
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is DbUpdateException or SqlException)
            {
                _logger.LogError(ex, "Không thể tạo phim");
                ModelState.AddModelError(string.Empty,
                    "Không thể lưu phim. Vui lòng kiểm tra và thử lại.");
            }

            await LoadGenresAsync(selectedGenreIds);
            return View(movie);
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

            // Kiểm tra các trường nhập trước khi mở giao dịch.
            if (!ModelState.IsValid)
            {
                await LoadGenresAsync(selectedGenreIds);
                return View(movie);
            }

            try
            {
                // Giữ việc kiểm tra lịch và sửa phim trong cùng giao dịch.
                await using var transaction =
                    await _context.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var existingMovie = await _context.Movies
                    .Include(m => m.Genres)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (existingMovie == null)
                {
                    return NotFound();
                }

                var genres = await ValidateGenresAsync(selectedGenreIds);

                // Không cho ngày khởi chiếu muộn hơn suất chiếu sớm nhất.
                var earliestStartTime = await _context.Showtimes
                    .Where(s => s.MovieId == id)
                    .Select(s => (DateTime?)s.StartTime)
                    .MinAsync();

                if (earliestStartTime.HasValue &&
                    movie.ReleaseDate.Date > earliestStartTime.Value.Date)
                {
                    ModelState.AddModelError(
                        nameof(Movie.ReleaseDate),
                        $"Ngày khởi chiếu không được sau ngày " +
                        $"{earliestStartTime.Value:dd/MM/yyyy} " +
                        "vì phim đã có suất chiếu vào ngày này.");
                }


                if (movie.DurationMinutes != existingMovie.DurationMinutes)
                {
                    var now = DateTimeOffset.UtcNow
                        .ToOffset(TimeSpan.FromHours(7))
                        .DateTime;

                    var hasFutureShowtimes = await _context.Showtimes
                        .AnyAsync(s =>
                            s.MovieId == id &&
                            s.StartTime > now);

                    if (hasFutureShowtimes)
                    {
                        ModelState.AddModelError(
                            nameof(Movie.DurationMinutes),
                            "Không thể đổi thời lượng vì phim còn suất chiếu "
                            + "trong tương lai. Hãy giữ nguyên thời lượng hiện tại.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    await LoadGenresAsync(selectedGenreIds);
                    return View(movie);
                }

                existingMovie.Title = movie.Title;
                existingMovie.Description = movie.Description;
                existingMovie.DurationMinutes = movie.DurationMinutes;
                existingMovie.ReleaseDate = movie.ReleaseDate;
                existingMovie.PosterUrl = movie.PosterUrl;
                existingMovie.IsActive = movie.IsActive;

                existingMovie.Genres.Clear();

                foreach (var genre in genres)
                {
                    existingMovie.Genres.Add(genre);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Xung đột cập nhật dữ liệu");
                ModelState.AddModelError(
                    string.Empty,
                    "Phim đã được thay đổi hoặc xóa bởi người khác. "
                    + "Vui lòng tải lại trang.");
            }
            catch (Exception ex)
                when (ex.GetBaseException() is SqlException sql &&
                      sql.Number == 1205)
            {
                _logger.LogWarning(ex, "Xung đột giao dịch dữ liệu");
                ModelState.AddModelError(
                    string.Empty,
                    "Có người đang cập nhật phim hoặc lịch chiếu cùng lúc. "
                    + "Vui lòng thử lại.");
            }
            catch (Exception ex) when (ex is DbUpdateException or SqlException)
            {
                _logger.LogError(ex, "Không thể lưu thay đổi dữ liệu");
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể lưu phim. Dữ liệu liên quan có thể đã thay đổi. "
                    + "Vui lòng tải lại trang và kiểm tra.");
            }

            await LoadGenresAsync(selectedGenreIds);
            return View(movie);
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
            var movie = await _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var hasShowtimes = await _context.Showtimes
                .AnyAsync(s => s.MovieId == id);

            if (hasShowtimes)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể xóa phim đã có lịch chiếu. "
                    + "Bạn có thể vào trang Sửa và bỏ chọn Hiển thị phim.");

                return View("Delete", movie);
            }

            try
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Xung đột cập nhật dữ liệu");
                if (!await _context.Movies.AnyAsync(m => m.Id == id))
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Dữ liệu phim đã thay đổi. Vui lòng tải lại trang.");
            }
            catch (Exception ex) when (ex is DbUpdateException or SqlException)
            {
                _logger.LogError(ex, "Không thể lưu thay đổi dữ liệu");
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể xóa phim. Dữ liệu có thể vừa được sử dụng "
                    + "trong lịch chiếu. Vui lòng tải lại trang và kiểm tra.");
            }

            return View("Delete", movie);
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