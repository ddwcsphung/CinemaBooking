using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Models;

public class Movie
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên phim.")]
    [StringLength(200, ErrorMessage = "Tên phim tối đa 200 ký tự.")]
    [Display(Name = "Tên phim")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập nội dung phim.")]
    [StringLength(4000, ErrorMessage = "Nội dung tối đa 4000 ký tự.")]
    [Display(Name = "Nội dung")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 600, ErrorMessage = "Thời lượng phải từ 1 đến 600 phút.")]
    [Display(Name = "Thời lượng (phút)")]
    public int DurationMinutes { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày khởi chiếu")]
    public DateTime ReleaseDate { get; set; } = DateTime.Today;

    [StringLength(500, ErrorMessage = "Đường dẫn ảnh poster tối đa 500 ký tự.")]
    [Display(Name = "Đường dẫn ảnh poster")]
    public string? PosterUrl { get; set; }

    [Display(Name = "Hiển thị phim")]
    public bool IsActive { get; set; } = true;

    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
