using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Models;

public enum BookingPaymentStatus
{
    [Display(Name = "Chưa thanh toán")]
    Unpaid = 0,

    [Display(Name = "Đã thanh toán")]
    Paid = 1
}

public class Booking
{
    public int Id { get; set; }

    [Required]
    [StringLength(32)]
    [Display(Name = "Mã đặt vé")]
    public string BookingCode { get; set; } =
        Guid.NewGuid().ToString("N").ToUpperInvariant();

    [Required]
    public string UserId { get; set; } = string.Empty;

    public IdentityUser User { get; set; } = null!;

    public int ShowtimeId { get; set; }

    public Showtime Showtime { get; set; } = null!;

    [Display(Name = "Thời điểm đặt")]
    public DateTimeOffset CreatedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;

    [Display(Name = "Tổng tiền")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "Thanh toán")]
    public BookingPaymentStatus PaymentStatus { get; set; } =
        BookingPaymentStatus.Unpaid;

    public ICollection<BookingSeat> BookingSeats { get; set; } =
        new List<BookingSeat>();
}
