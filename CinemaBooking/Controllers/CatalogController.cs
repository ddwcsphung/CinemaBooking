using CinemaBooking.Data;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers;

public class CatalogController : Controller
{
    private readonly ApplicationDbContext _context;

    private const int PageSize = 8;

    private static DateTime CinemaNow =>
        DateTimeOffset.UtcNow
            .ToOffset(TimeSpan.FromHours(7))
            .DateTime;

    public CatalogController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? keyword,
        int? genreId,
        int page = 1)
    {
        keyword = keyword?.Trim();

        var query = _context.Movies
            .AsNoTracking()
            .Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(m => m.Title.Contains(keyword));
        }

        if (genreId.HasValue && genreId.Value > 0)
        {
            query = query.Where(m =>
                m.Genres.Any(g => g.Id == genreId.Value));
        }
        else
        {
            genreId = null;
        }

        var totalItems = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(
            totalItems / (double)PageSize);

        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var movies = await query
            .Include(m => m.Genres)
            .OrderByDescending(m => m.ReleaseDate)
            .ThenBy(m => m.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var genres = await _context.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync();

        var model = new MovieCatalogViewModel
        {
            Movies = movies,
            Keyword = keyword,
            GenreId = genreId,
            Page = page,
            TotalItems = totalItems,
            TotalPages = totalPages,

            GenreOptions = genres.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(
        int id,
        DateTime? date)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var movie = await _context.Movies
            .AsNoTracking()
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m =>
                m.Id == id && m.IsActive);

        if (movie == null)
        {
            return NotFound();
        }

        var now = CinemaNow;
        var selectedDate = (date ?? now).Date;

        if (selectedDate < now.Date)
        {
            selectedDate = now.Date;
        }

        if (selectedDate == DateTime.MaxValue.Date)
        {
            return BadRequest();
        }

        var nextDate = selectedDate.AddDays(1);

        var showtimes = await _context.Showtimes
            .AsNoTracking()
            .Include(s => s.Room)
            .Where(s =>
                s.MovieId == id &&
                s.IsActive &&
                s.Room.IsActive &&
                s.Room.Seats.Any(seat => seat.IsActive) &&
                s.StartTime > now &&
                s.StartTime >= selectedDate &&
                s.StartTime < nextDate)
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.Id)
            .ToListAsync();

        var model = new MovieDetailsViewModel
        {
            Movie = movie,
            SelectedDate = selectedDate,
            Showtimes = showtimes
        };

        return View(model);
    }
}
