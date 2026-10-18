USE [CinemaBooking_Submission];
GO
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET CONCAT_NULL_YIELDS_NULL ON; SET ARITHABORT ON; SET NUMERIC_ROUNDABORT OFF;
GO
SET NOCOUNT ON;
IF (SELECT COUNT(*) FROM dbo.Movies) < 10 THROW 51000, 'Thieu phim demo.', 1;
IF (SELECT COUNT(*) FROM dbo.AspNetUsers WHERE Email LIKE N'demo.%@example.com') <> 3 THROW 51000, 'Thieu tai khoan demo.', 1;
IF EXISTS (SELECT ShowtimeId,SeatId FROM dbo.BookingSeats GROUP BY ShowtimeId,SeatId HAVING COUNT(*) > 1) THROW 51000, 'Trung ghe.', 1;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE is_disabled=1 OR is_not_trusted=1) THROW 51000, 'Khoa ngoai khong hop le.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles ur JOIN dbo.AspNetUsers u ON u.Id=ur.UserId JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId WHERE u.Email=N'demo.admin@example.com' AND r.Name=N'Admin') THROW 51000, 'Thieu quyen Admin.', 1;
DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS;
SELECT N'Movies' AS Bang,COUNT(*) AS SoLuong FROM dbo.Movies
UNION ALL SELECT N'Rooms',COUNT(*) FROM dbo.Rooms
UNION ALL SELECT N'Seats',COUNT(*) FROM dbo.Seats
UNION ALL SELECT N'Showtimes',COUNT(*) FROM dbo.Showtimes
UNION ALL SELECT N'Bookings',COUNT(*) FROM dbo.Bookings;
SELECT N'DATABASE_OK' AS KetQua;
GO