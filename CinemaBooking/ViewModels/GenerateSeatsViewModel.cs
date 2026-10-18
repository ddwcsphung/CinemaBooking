using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.ViewModels;

public class GenerateSeatsViewModel
{
    [Range(1, int.MaxValue)]
    public int RoomId { get; set; }

    [Range(1, 26, ErrorMessage = "Số hàng phải từ 1 đến 26.")]
    [Display(Name = "Số hàng ghế")]
    public int RowCount { get; set; } = 5;

    [Range(1, 30, ErrorMessage = "Mỗi hàng phải có từ 1 đến 30 ghế.")]
    [Display(Name = "Số ghế mỗi hàng")]
    public int SeatsPerRow { get; set; } = 8;
}
