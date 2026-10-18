# CinemaBooking

Ứng dụng đặt vé xem phim ASP.NET Core MVC, .NET 10, Identity, EF Core và SQL Server.

## Chạy trên Windows

1. Cài .NET 10 SDK, Visual Studio hỗ trợ .NET 10 với workload **ASP.NET and web development**, và SQL Server Express LocalDB.
2. Clone repo, mở `CinemaBooking.slnx`, restore NuGet và build.
3. Kiểm tra `ConnectionStrings:DefaultConnection`. Mặc định dùng `(localdb)\MSSQLLocalDB` và database phát triển riêng trên máy.
4. Trong Package Manager Console, chọn project `CinemaBooking`, chạy `Update-Database`. Terminal có công cụ EF tương ứng cũng có thể dùng `dotnet ef database update --project CinemaBooking`.
5. Chạy project `CinemaBooking` bằng profile `https`.

```powershell
git clone https://github.com/ddwcsphung/CinemaBooking.git
cd CinemaBooking
dotnet restore
dotnet build CinemaBooking.slnx
dotnet run --project CinemaBooking --launch-profile https
```

Không tạo migration mới chỉ để cài đặt. Không đưa database, mật khẩu, token hoặc User Secrets lên Git. Trên máy không dùng LocalDB, cấu hình `DefaultConnection` đến SQL Server riêng qua User Secrets hoặc biến môi trường `ConnectionStrings__DefaultConnection`.

## Dữ liệu demo

Trong Visual Studio: nhấn phải project ứng dụng → **Manage User Secrets**, thêm các trường dưới đây vào JSON hiện có. Thay giá trị mật khẩu bằng mật khẩu thử riêng đáp ứng chính sách Identity.

```json
{
  "DemoData": {
    "Enabled": true,
    "Password": "MAT_KHAU_THU_RIENG_CO_CHU_HOA_SO_VA_KY_TU_DAC_BIET"
  }
}
```

Chạy ứng dụng ở **Development**. Seeder áp dụng migration hiện có và tạo:

- `demo.admin@example.com`: Admin.
- `demo.a@example.com`, `demo.b@example.com`: hai khách hàng.
- 10 phim hư cấu, gồm một phim ẩn; 3 thể loại; 4 phòng với trạng thái khác nhau; ghế, lịch chiếu và đơn chưa/đã thanh toán.
- Poster minh họa gốc trong `wwwroot/images/posters`.

Lịch demo dùng ngày Việt Nam hiện tại cộng 1–2 ngày. Chạy lại trong cùng ngày không tạo trùng; ngày mới chỉ bổ sung lịch, không đổi suất hoặc vé cũ. Seeder không xóa dữ liệu hiện có hoặc đặt lại mật khẩu tài khoản đã tồn tại. Nên dùng database riêng cho demo và đặt `DemoData:Enabled` về `false` sau khi khởi tạo.

Không bật demo trong production; code chỉ gọi seeder khi môi trường Development. Cookie đăng nhập và session được lưu riêng; session hết hạn sau 20 phút không hoạt động, dữ liệu trong bộ nhớ mất khi ứng dụng khởi động lại.

## Dùng tài khoản Admin của riêng bạn

Đăng ký qua website, dừng ứng dụng, thêm vào User Secrets:

```json
{ "SeedAdmin": { "Email": "email-ban-vua-dang-ky@example.com" } }
```

Khởi động lại ở Development, đăng xuất và đăng nhập lại. Seeder không tạo tài khoản khi email chưa tồn tại. Thay email cấu hình không tự thu hồi Admin đã cấp trước đó.

## Chức năng

- Khách: tìm phim theo tên/thể loại, phân trang, xem lịch theo ngày, đăng ký/đăng nhập, chọn ghế, đặt vé, xem vé của mình, quản lý hồ sơ và mật khẩu.
- Admin: phim, thể loại, phòng, tạo ghế, bật/tắt ghế, tạo/sửa/xóa/đóng mở bán suất chiếu, lọc đơn và xác nhận thanh toán tại quầy.
- API trạng thái ghế được gọi mỗi 5 giây; session ghi nhớ lựa chọn nhưng không giữ chỗ.
- Giá và điều kiện ghế được kiểm tra ở server. Một ghế chỉ thuộc một đơn trong cùng suất nhờ transaction và unique index.
- Suất đã có đơn hoặc đã bắt đầu không được sửa/xóa; đóng bán giữ nguyên vé cũ. Khoảng cách hai suất cùng phòng tối thiểu 15 phút dọn phòng.
- Trang lỗi HTML dùng layout chung; API giữ mã HTTP và dữ liệu JSON phù hợp.

Phạm vi thanh toán là xác nhận tại quầy, chưa tích hợp cổng thanh toán trực tuyến, email xác nhận, hủy/hoàn tiền. Các liên kết cho luồng email chưa triển khai được ẩn khỏi menu tài khoản.

## Kiểm thử tự động

```powershell
dotnet build CinemaBooking.slnx
dotnet run --project tests/CinemaBooking.SmokeTests --no-build
```

Bộ kiểm thử tạo **database mới có hậu tố ngẫu nhiên**, áp dụng migration, tạo dữ liệu thử, khởi chạy ứng dụng ở cổng localhost riêng, gửi HTTP thật và tự xóa database thử sau khi kết thúc. Không dùng database hoạt động của nhóm. Tài khoản SQL phải được phép tạo/xóa database thử.

Mặc định dùng LocalDB. Để kiểm tra bằng SQL Server khác, cấu hình biến môi trường `CINEMA_TEST_CONNECTION` bằng chuỗi kết nối nền; chương trình luôn thay tên database bằng tên thử ngẫu nhiên. Không ghi mật khẩu vào lệnh được chia sẻ hoặc tài liệu.

Các biến tùy chọn `CINEMA_TEST_RESULTS` và `CINEMA_TEST_LOG` chỉ định file JSON kết quả và log khi thất bại. Kết quả kiểm thử, giới hạn và kịch bản demo ở `docs/kiem-thu.md`.

## Làm việc với Git

Pull `master` trước khi tạo nhánh. Commit theo nhóm thay đổi, push nhánh riêng, tạo PR vào `master`. Sau khi merge, các máy khác phải pull để nhận source mới; dữ liệu SQL Server không tự đồng bộ qua Git.

```powershell
git switch master
git pull --ff-only origin master
git switch -c feature/ten-cong-viec
```

Xem `docs/kien-truc.md` để biết luồng nghiệp vụ, ERD và hợp đồng API.
