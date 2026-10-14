using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Models;

public class Room
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên phòng.")]
    [StringLength(100, ErrorMessage = "Tên phòng tối đa 100 ký tự.")]
    [Display(Name = "Tên phòng")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Đang hoạt động")]
    public bool IsActive { get; set; } = true;

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}