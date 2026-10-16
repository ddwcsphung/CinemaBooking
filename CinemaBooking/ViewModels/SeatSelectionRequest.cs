using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.ViewModels;

public class SeatSelectionRequest
{
    [Required]
    [MaxLength(10, ErrorMessage = "Chỉ được chọn tối đa 10 ghế.")]
    public List<int> SeatIds { get; set; } = new();
}