using CinemaBooking.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaBooking.ViewModels;

public class MovieCatalogViewModel
{
    public List<Movie> Movies { get; set; } = new();

    public string? Keyword { get; set; }

    public int? GenreId { get; set; }

    public List<SelectListItem> GenreOptions { get; set; } = new();

    public int Page { get; set; } = 1;

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }
}