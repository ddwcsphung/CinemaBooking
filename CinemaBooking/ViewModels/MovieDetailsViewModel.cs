using CinemaBooking.Models;

namespace CinemaBooking.ViewModels;

public class MovieDetailsViewModel
{
    public Movie Movie { get; set; } = null!;

    public DateTime SelectedDate { get; set; }

    public List<Showtime> Showtimes { get; set; } = new();
}