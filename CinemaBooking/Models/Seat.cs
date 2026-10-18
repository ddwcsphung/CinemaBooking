using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Models;

public class Seat
{
    public int Id { get; set; }

    [Display(Name = "Phòng chiếu")]
    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập hàng ghế.")]
    [RegularExpression(
        @"^[A-Z]{1,2}$",
        ErrorMessage = "Hàng ghế gồm 1–2 chữ cái viết hoa, ví dụ A hoặc AA.")]
    [StringLength(2)]
    [Display(Name = "Hàng ghế")]
    public string RowLabel { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Số ghế phải từ 1 đến 100.")]
    [Display(Name = "Số ghế")]
    public int SeatNumber { get; set; }

    [Display(Name = "Đang sử dụng")]
    public bool IsActive { get; set; } = true;
}
