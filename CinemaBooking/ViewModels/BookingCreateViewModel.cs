using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.ViewModels;

public class BookingCreateViewModel
{
    public int ShowtimeId { get; set; }

    public string MovieTitle { get; set; } = string.Empty;

    public string RoomName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public decimal BasePrice { get; set; }

    [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một ghế.")]
    [MaxLength(10, ErrorMessage = "Mỗi lần chỉ được đặt tối đa 10 ghế.")]
    public List<int> SelectedSeatIds { get; set; } = new();

    public List<BookingSeatViewModel> Seats { get; set; } = new();
}

public class BookingSeatViewModel
{
    public int SeatId { get; set; }

    public string RowLabel { get; set; } = string.Empty;

    public int SeatNumber { get; set; }

    public bool IsActive { get; set; }

    public bool IsBooked { get; set; }

    public string Label => $"{RowLabel}{SeatNumber}";
}
