using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaBooking.ViewModels;

public class ShowtimeCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phim.")]
    [Display(Name = "Phim")]
    public int MovieId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng.")]
    [Display(Name = "Phòng chiếu")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu.")]
    [Display(Name = "Giờ bắt đầu")]
    public DateTime? StartTime { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá vé.")]
    [Range(
        typeof(decimal),
        "1000",
        "1000000",
        ErrorMessage = "Giá vé từ 1.000 đến 1.000.000 đồng.")]
    [Display(Name = "Giá vé (đồng)")]
    public decimal? BasePrice { get; set; } = 80000;

    [Display(Name = "Mở bán")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> MovieOptions { get; set; } = new();

    public List<SelectListItem> RoomOptions { get; set; } = new();
}
