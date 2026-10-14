using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Models;

public class Showtime
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phim.")]
    [Display(Name = "Phim")]
    public int MovieId { get; set; }

    public Movie Movie { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng.")]
    [Display(Name = "Phòng chiếu")]
    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    [Display(Name = "Giờ bắt đầu")]
    public DateTime StartTime { get; set; }

    [Display(Name = "Giờ kết thúc")]
    public DateTime EndTime { get; set; }

    [Range(
        typeof(decimal),
        "1000",
        "1000000",
        ErrorMessage = "Giá vé phải từ 1.000 đến 1.000.000 đồng.")]
    [Display(Name = "Giá vé")]
    public decimal BasePrice { get; set; }

    [Display(Name = "Mở bán")]
    public bool IsActive { get; set; } = true;
}