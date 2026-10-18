using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Areas.Identity.Pages;

public static class IdentityErrorMessages
{
    public static string Translate(IdentityError error) => error.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" =>
            "Email này đã được dùng để đăng ký tài khoản khác.",
        "InvalidEmail" or "InvalidUserName" =>
            "Email không hợp lệ.",
        "PasswordTooShort" =>
            "Mật khẩu phải có ít nhất 6 ký tự.",
        "PasswordRequiresDigit" =>
            "Mật khẩu phải có ít nhất một chữ số (0-9).",
        "PasswordRequiresLower" =>
            "Mật khẩu phải có ít nhất một chữ thường (a-z).",
        "PasswordRequiresUpper" =>
            "Mật khẩu phải có ít nhất một chữ hoa (A-Z).",
        "PasswordRequiresNonAlphanumeric" =>
            "Mật khẩu phải có ít nhất một ký tự đặc biệt (ví dụ: ! @ # $).",
        "PasswordRequiresUniqueChars" =>
            "Mật khẩu phải có nhiều ký tự khác nhau hơn.",
        "PasswordMismatch" =>
            "Mật khẩu hiện tại không đúng.",
        "ConcurrencyFailure" =>
            "Tài khoản vừa được thay đổi ở nơi khác. Vui lòng tải lại trang và thử lại.",
        _ => error.Description
    };
}
