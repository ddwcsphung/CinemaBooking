using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.ViewModels;

public class BookingRequestViewModel
{
    [Range(1, int.MaxValue)]
    public int ShowtimeId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(10)]
    public List<int> SelectedSeatIds { get; set; } = new();
}
