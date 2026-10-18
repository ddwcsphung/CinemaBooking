# Kiểm thử và demo

## Kết quả ngày 18/10/2026

- 62 kiểm tra HTTP/SQL tự động đều đạt; chi tiết trong `ket-qua-kiem-thu.json`.
- Build solution: 0 lỗi, 0 cảnh báo. NuGet audit bao gồm dependency gián tiếp: không ghi nhận gói có lỗ hổng đã biết tại thời điểm kiểm tra.
- Catalog ở 375, 768 và 1366 px không tràn ngang; ảnh poster tải đủ, menu điện thoại mở/đóng được. Ảnh thực tế: `images/catalog-mobile.png`, `images/catalog-desktop.png`.
- Dùng database LocalDB riêng cho kiểm thử; không chạy seed hay thay dữ liệu trong database làm việc của nhóm.

## Cách tái hiện

Build solution rồi chạy `dotnet run --project tests/CinemaBooking.SmokeTests --no-build` từ thư mục repo. Bộ test dùng SQL Server thật, database riêng tự tạo/xóa và HTTP localhost; không giả lập transaction bằng SQLite/InMemory. Cổng được chọn tự động, tài khoản và mật khẩu thử sinh riêng mỗi lượt.

Các nhóm được kiểm tra:

- Tạo database từ migration và seed chạy lại không trùng.
- Đăng nhập Admin và hai khách, ngăn khách gọi quản trị và đọc đơn người khác.
- Trang lỗi 400/404 có layout, API chưa đăng nhập trả 401 và API lỗi không bị thay thành HTML.
- JSON selection thiếu trường/null/lỗi/trùng/âm/quá 10; antiforgery; khôi phục, bỏ chọn và tách tài khoản.
- 10 lượt hai khách đặt ghế khác nhau cùng suất, cùng ghế chỉ một người thành công, cùng ghế vật lý ở hai suất khác nhau.
- Giá server, xóa session sau đặt, định dạng ngày, xác nhận thanh toán đồng thời và lặp lại.
- Đóng bán lặp, từ chối đặt suất vừa đóng và hiện thông báo trên Catalog; chặn sửa/xóa suất có đơn.
- SQL trigger thử nghiệm gây lỗi giữa giao dịch để kiểm tra rollback; kiểm tra unique index không có vé trùng.
- Ảnh poster JPEG có thật và phòng không có ghế hoạt động không mở trang đặt vé.

Bộ test không thay thế mọi ca của báo cáo TC01–TC60 trước đó. Không kết luận một ca đạt nếu chưa có bằng chứng tương ứng trên phiên bản đang dùng. Kết quả JSON/log có thể xuất bằng biến `CINEMA_TEST_RESULTS`, `CINEMA_TEST_LOG`.

## Checklist giao diện thủ công

Kiểm tra 360/375, 768 và 1366 px: menu thu gọn, không tràn toàn trang, bảng/sơ đồ ghế cuộn trong vùng, nút không bị cắt và footer không đè nội dung. Thử Tab/Space ở sơ đồ ghế, nhãn và trạng thái ngoài màu sắc.

Mở hai phiên thường/ẩn danh: A chọn chưa đặt; B đặt ghế đó; A tự bỏ chọn sau lần cập nhật kế tiếp. F5 sau thông báo đã lưu phải khôi phục đúng; bỏ chọn toàn bộ rồi F5 phải rỗng. Mất mạng khóa đặt vé và phục hồi khi có mạng. Session nhàn rỗi trên 20 phút hết hạn; không refresh liên tục trong lúc đo hết hạn.

## Kịch bản demo

1. Bật dữ liệu demo trên database riêng theo README, đăng nhập hai khách và Admin ở các phiên độc lập.
2. Tìm một phim, lọc thể loại, chuyển trang, xem lịch ngày mai và chọn một suất chưa có đơn.
3. Chọn 2 ghế, tải lại để minh họa session; dùng khách B đặt một ghế của A để minh họa AJAX.
4. A đặt ghế còn lại; mở chi tiết và lịch sử vé.
5. Admin tìm mã đơn và xác nhận thanh toán, bấm lại để minh họa thao tác lặp an toàn; khách tải lại thấy đã thanh toán.
6. Admin thử sửa/xóa suất đã có đơn, tạo lịch trùng, tắt ghế có vé tương lai và mở một URL không tồn tại.

## Phạm vi bản sửa

BUG-01: khóa suất trước khi kiểm tra/chèn ghế, thử lại transaction chỉ khi SQL Server báo deadlock. BUG-02: thông báo giới hạn poster tiếng Việt. BUG-03: middleware và action trang lỗi HTML. BUG-04: suất không còn bán chuyển về Catalog kèm thông báo. BUG-05: phân biệt trường seatIds bị thiếu/null và mảng rỗng. BUG-06: bổ sung poster minh họa gốc ở các đường dẫn dữ liệu cũ. OBS-01: seeder theo ngày hiện tại, tài khoản example.com và hướng dẫn tái tạo. OBS-02: từ chối form đặt vé khi phòng không có ghế sử dụng.

Không chỉnh schema hoặc migration cũ trong đợt sửa này. Giữ nguyên dữ liệu thật, chỉ seed khi được bật trong Development. Chú thích giảng giải trong mã ứng dụng được bỏ, giữ ghi chú ngắn ở quy tắc khóa và dữ liệu lịch.
