using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Models;

public class BookingSeat
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    public int ShowtimeId { get; set; }

    public int SeatId { get; set; }

    public Seat Seat { get; set; } = null!;

    // Giữ giá tại thời điểm đặt, dù giá suất chiếu thay đổi sau đó.
    [Display(Name = "Giá vé")]
    public decimal UnitPrice { get; set; }
}