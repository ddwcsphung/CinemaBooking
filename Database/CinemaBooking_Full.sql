IF DB_ID(N'CinemaBooking_Submission') IS NULL
    EXEC(N'CREATE DATABASE [CinemaBooking_Submission]');
GO
USE [CinemaBooking_Submission];
GO
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET CONCAT_NULL_YIELDS_NULL ON; SET ARITHABORT ON; SET NUMERIC_ROUNDABORT OFF;
GO
USE [CinemaBooking_Submission];
GO
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET CONCAT_NULL_YIELDS_NULL ON; SET ARITHABORT ON; SET NUMERIC_ROUNDABORT OFF;
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000000_CreateIdentitySchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'00000000000000_CreateIdentitySchema', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012104600_InitialIdentity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261012104600_InitialIdentity', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012124101_AddMoviesAndGenres'
)
BEGIN
    CREATE TABLE [Genres] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Genres] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012124101_AddMoviesAndGenres'
)
BEGIN
    CREATE TABLE [Movies] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(4000) NOT NULL,
        [DurationMinutes] int NOT NULL,
        [ReleaseDate] datetime2 NOT NULL,
        [PosterUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Movies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012124101_AddMoviesAndGenres'
)
BEGIN
    CREATE TABLE [GenreMovie] (
        [GenresId] int NOT NULL,
        [MoviesId] int NOT NULL,
        CONSTRAINT [PK_GenreMovie] PRIMARY KEY ([GenresId], [MoviesId]),
        CONSTRAINT [FK_GenreMovie_Genres_GenresId] FOREIGN KEY ([GenresId]) REFERENCES [Genres] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GenreMovie_Movies_MoviesId] FOREIGN KEY ([MoviesId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012124101_AddMoviesAndGenres'
)
BEGIN
    CREATE INDEX [IX_GenreMovie_MoviesId] ON [GenreMovie] ([MoviesId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012124101_AddMoviesAndGenres'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261012124101_AddMoviesAndGenres', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013071814_AddRoomsAndSeats'
)
BEGIN
    CREATE TABLE [Rooms] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013071814_AddRoomsAndSeats'
)
BEGIN
    CREATE TABLE [Seats] (
        [Id] int NOT NULL IDENTITY,
        [RoomId] int NOT NULL,
        [RowLabel] nvarchar(2) NOT NULL,
        [SeatNumber] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Seats] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Seats_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013071814_AddRoomsAndSeats'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Seats_RoomId_RowLabel_SeatNumber] ON [Seats] ([RoomId], [RowLabel], [SeatNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013071814_AddRoomsAndSeats'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261013071814_AddRoomsAndSeats', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013092347_AddShowtimes'
)
BEGIN
    CREATE TABLE [Showtimes] (
        [Id] int NOT NULL IDENTITY,
        [MovieId] int NOT NULL,
        [RoomId] int NOT NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [BasePrice] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Showtimes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Showtimes_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Showtimes_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013092347_AddShowtimes'
)
BEGIN
    CREATE INDEX [IX_Showtimes_MovieId] ON [Showtimes] ([MovieId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013092347_AddShowtimes'
)
BEGIN
    CREATE INDEX [IX_Showtimes_RoomId_StartTime] ON [Showtimes] ([RoomId], [StartTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013092347_AddShowtimes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261013092347_AddShowtimes', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    CREATE TABLE [Bookings] (
        [Id] int NOT NULL IDENTITY,
        [BookingCode] nvarchar(32) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ShowtimeId] int NOT NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [PaymentStatus] int NOT NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Bookings_Id_ShowtimeId] UNIQUE ([Id], [ShowtimeId]),
        CONSTRAINT [FK_Bookings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Bookings_Showtimes_ShowtimeId] FOREIGN KEY ([ShowtimeId]) REFERENCES [Showtimes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    CREATE TABLE [BookingSeats] (
        [Id] int NOT NULL IDENTITY,
        [BookingId] int NOT NULL,
        [ShowtimeId] int NOT NULL,
        [SeatId] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_BookingSeats] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BookingSeats_Bookings_BookingId_ShowtimeId] FOREIGN KEY ([BookingId], [ShowtimeId]) REFERENCES [Bookings] ([Id], [ShowtimeId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BookingSeats_Seats_SeatId] FOREIGN KEY ([SeatId]) REFERENCES [Seats] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bookings_BookingCode] ON [Bookings] ([BookingCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    CREATE INDEX [IX_Bookings_ShowtimeId] ON [Bookings] ([ShowtimeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    CREATE INDEX [IX_Bookings_UserId_CreatedAtUtc] ON [Bookings] ([UserId], [CreatedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    CREATE INDEX [IX_BookingSeats_BookingId_ShowtimeId] ON [BookingSeats] ([BookingId], [ShowtimeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    CREATE INDEX [IX_BookingSeats_SeatId] ON [BookingSeats] ([SeatId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BookingSeats_ShowtimeId_SeatId] ON [BookingSeats] ([ShowtimeId], [SeatId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030728_AddBookingsAndBookingSeats'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014030728_AddBookingsAndBookingSeats', N'10.0.12');
END;

COMMIT;
GO


USE [CinemaBooking_Submission];
GO
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET CONCAT_NULL_YIELDS_NULL ON; SET ARITHABORT ON; SET NUMERIC_ROUNDABORT OFF;
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
IF EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'demo.admin@example.com') BEGIN PRINT N'Du lieu demo da ton tai; khong ghi de.'; RETURN; END;
IF EXISTS (SELECT 1 FROM dbo.AspNetUsers) OR EXISTS (SELECT 1 FROM dbo.Movies) OR EXISTS (SELECT 1 FROM dbo.Rooms) THROW 51000, 'Chi nap demo vao database moi, khong ghi de du lieu hien co.', 1;
DECLARE @Today date = CONVERT(date, SWITCHOFFSET(SYSDATETIMEOFFSET(), '+07:00'));
BEGIN TRY
BEGIN TRANSACTION;
INSERT INTO dbo.[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp]) VALUES (N'2774726c-b2c8-4069-9f33-036e646e08cc',N'Customer',N'CUSTOMER',N'8181a6d8-428f-438e-887f-55dfe15a5512');
INSERT INTO dbo.[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp]) VALUES (N'3279b9d7-bca0-4725-bd11-41f76bfa193b',N'Admin',N'ADMIN',N'89880bcb-7f2a-40f1-9c62-f7bb2b493368');
INSERT INTO dbo.[AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnd],[LockoutEnabled],[AccessFailedCount]) VALUES (N'91e2ccd2-d6a4-4124-9e94-1213eef9f450',N'demo.admin@example.com',N'DEMO.ADMIN@EXAMPLE.COM',N'demo.admin@example.com',N'DEMO.ADMIN@EXAMPLE.COM',1,N'AQAAAAIAAYagAAAAEEi8IQMsgssR87T6S7z6ZtpUzbngmfp1mqA0UrBEYjJ4dLkd+t2OZ+iaqwiuMb11SQ==',N'IQT5DABGB3OGBROQJQQGYNPFMPRBIY6A',N'bcef8668-0b87-41ab-8b47-f686a1797a8d',NULL,0,0,NULL,1,0);
INSERT INTO dbo.[AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnd],[LockoutEnabled],[AccessFailedCount]) VALUES (N'bbe2a506-433c-47cd-bcb5-7aba2faee1f0',N'demo.a@example.com',N'DEMO.A@EXAMPLE.COM',N'demo.a@example.com',N'DEMO.A@EXAMPLE.COM',1,N'AQAAAAIAAYagAAAAEKnrCTZ9X3/5GH84dJkQaFnU0ld5sYAtZmtr3RqS4sVjVO9VONWvxIreuN85FX6Wjw==',N'HKRSBBQ62D3XZCKCPDDO44HEFEYDUW2S',N'ab494382-eb5f-4b75-8181-b3cb643802dc',NULL,0,0,NULL,1,0);
INSERT INTO dbo.[AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnd],[LockoutEnabled],[AccessFailedCount]) VALUES (N'ce615838-6c01-4b6a-81e0-88eb8618bdbc',N'demo.b@example.com',N'DEMO.B@EXAMPLE.COM',N'demo.b@example.com',N'DEMO.B@EXAMPLE.COM',1,N'AQAAAAIAAYagAAAAEBTZG/hfcSKlzDYEhitWQr0bKv+IJn/tsrMvljI5XXeWr+qc3wrlgOymHXCG8zh7AQ==',N'TJSSQBFXJY5PBA33Y4QA2OZJIW6FRTVJ',N'abadc19b-053f-4667-be9f-a060e79a6497',NULL,0,0,NULL,1,0);
SET IDENTITY_INSERT dbo.[Genres] ON;
INSERT INTO dbo.[Genres] ([Id],[Name]) VALUES (1,N'Hành động');
INSERT INTO dbo.[Genres] ([Id],[Name]) VALUES (2,N'Phiêu lưu');
INSERT INTO dbo.[Genres] ([Id],[Name]) VALUES (3,N'Tình cảm');
SET IDENTITY_INSERT dbo.[Genres] OFF;
SET IDENTITY_INSERT dbo.[Movies] ON;
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (1,N'Hành trình bình minh',N'Một nhóm bạn vượt qua những thử thách để tìm lại người đồng đội mất tích và khám phá ý nghĩa của lòng tin.',90,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-01.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (2,N'Bí mật căn phòng cuối',N'Trở về căn nhà cũ, một cô gái phát hiện những lá thư dẫn đến bí mật mà gia đình đã giấu kín nhiều năm.',92,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-02.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (3,N'Chuyến tàu mùa hạ',N'Chuyến tàu cuối mùa hè đưa những người xa lạ đến với nhau và mở ra một khởi đầu mới.',94,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-03.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (4,N'Thành phố ánh sáng',N'Một kỹ sư trẻ cùng đội cứu hộ chạy đua với thời gian để đưa ánh sáng trở lại thành phố.',96,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-04.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (5,N'Hẹn ngày trở lại',N'Hai người bạn gặp lại sau nhiều năm và quyết định hoàn thành lời hẹn từ thời thanh xuân.',98,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-05.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (6,N'Phía bên kia đại dương',N'Hành trình vượt biển giúp một gia đình tìm lại sự gắn kết và niềm hy vọng vào ngày mai.',100,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-06.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (7,N'Dấu chân trên mây',N'Đoàn thám hiểm trẻ chinh phục đỉnh núi đầu tiên, vượt qua giới hạn của bản thân.',102,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-07.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (8,N'Những ngày rực rỡ',N'Những thành viên của một ban nhạc nhỏ cùng chuẩn bị buổi biểu diễn quan trọng nhất cuộc đời.',104,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-08.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (9,N'Lời hứa cuối mùa',N'Giữa những đổi thay của làng quê, một người trẻ trở về để gìn giữ ước mơ của gia đình.',106,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-09.jpg',1);
INSERT INTO dbo.[Movies] ([Id],[Title],[Description],[DurationMinutes],[ReleaseDate],[PosterUrl],[IsActive]) VALUES (10,N'Ngọn hải đăng xa',N'Người gác hải đăng và một vị khách bất ngờ cùng tìm lời giải cho tín hiệu bí ẩn ngoài khơi.',108,DATEADD(SECOND,0,CAST(DATEADD(DAY,-1,@Today) AS datetime2)),N'/images/posters/demo-10.jpg',0);
SET IDENTITY_INSERT dbo.[Movies] OFF;
SET IDENTITY_INSERT dbo.[Rooms] ON;
INSERT INTO dbo.[Rooms] ([Id],[Name],[IsActive]) VALUES (1,N'Demo R1',1);
INSERT INTO dbo.[Rooms] ([Id],[Name],[IsActive]) VALUES (2,N'Demo R2',0);
INSERT INTO dbo.[Rooms] ([Id],[Name],[IsActive]) VALUES (3,N'Demo R3',1);
INSERT INTO dbo.[Rooms] ([Id],[Name],[IsActive]) VALUES (4,N'Demo R4',1);
SET IDENTITY_INSERT dbo.[Rooms] OFF;
SET IDENTITY_INSERT dbo.[Seats] ON;
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (1,1,N'A',1,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (2,1,N'A',2,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (3,1,N'A',3,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (4,1,N'A',4,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (5,1,N'A',5,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (6,1,N'B',1,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (7,1,N'B',2,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (8,1,N'B',3,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (9,1,N'B',4,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (10,1,N'B',5,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (11,1,N'C',1,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (12,1,N'C',2,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (13,1,N'C',3,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (14,1,N'C',4,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (15,1,N'C',5,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (16,2,N'A',1,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (17,2,N'A',2,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (18,2,N'A',3,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (19,2,N'A',4,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (20,2,N'A',5,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (21,2,N'B',1,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (22,2,N'B',2,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (23,2,N'B',3,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (24,2,N'B',4,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (25,2,N'B',5,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (26,2,N'C',1,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (27,2,N'C',2,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (28,2,N'C',3,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (29,2,N'C',4,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (30,2,N'C',5,1);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (31,4,N'A',1,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (32,4,N'A',2,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (33,4,N'A',3,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (34,4,N'A',4,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (35,4,N'A',5,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (36,4,N'B',1,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (37,4,N'B',2,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (38,4,N'B',3,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (39,4,N'B',4,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (40,4,N'B',5,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (41,4,N'C',1,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (42,4,N'C',2,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (43,4,N'C',3,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (44,4,N'C',4,0);
INSERT INTO dbo.[Seats] ([Id],[RoomId],[RowLabel],[SeatNumber],[IsActive]) VALUES (45,4,N'C',5,0);
SET IDENTITY_INSERT dbo.[Seats] OFF;
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (1,1);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (2,2);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (3,3);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (1,4);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (2,5);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (3,6);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (1,7);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (2,8);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (3,9);
INSERT INTO dbo.[GenreMovie] ([GenresId],[MoviesId]) VALUES (1,10);
SET IDENTITY_INSERT dbo.[Showtimes] ON;
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (1,1,1,DATEADD(SECOND,32400,CAST(DATEADD(DAY,1,@Today) AS datetime2)),DATEADD(SECOND,37800,CAST(DATEADD(DAY,1,@Today) AS datetime2)),80000.00,1);
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (2,2,1,DATEADD(SECOND,43200,CAST(DATEADD(DAY,1,@Today) AS datetime2)),DATEADD(SECOND,48720,CAST(DATEADD(DAY,1,@Today) AS datetime2)),85000.00,1);
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (3,3,1,DATEADD(SECOND,54000,CAST(DATEADD(DAY,1,@Today) AS datetime2)),DATEADD(SECOND,59640,CAST(DATEADD(DAY,1,@Today) AS datetime2)),90000.00,1);
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (4,4,1,DATEADD(SECOND,64800,CAST(DATEADD(DAY,1,@Today) AS datetime2)),DATEADD(SECOND,70560,CAST(DATEADD(DAY,1,@Today) AS datetime2)),95000.00,1);
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (5,5,1,DATEADD(SECOND,32400,CAST(DATEADD(DAY,2,@Today) AS datetime2)),DATEADD(SECOND,38280,CAST(DATEADD(DAY,2,@Today) AS datetime2)),100000.00,1);
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (6,6,1,DATEADD(SECOND,43200,CAST(DATEADD(DAY,2,@Today) AS datetime2)),DATEADD(SECOND,49200,CAST(DATEADD(DAY,2,@Today) AS datetime2)),105000.00,1);
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (7,7,1,DATEADD(SECOND,54000,CAST(DATEADD(DAY,2,@Today) AS datetime2)),DATEADD(SECOND,60120,CAST(DATEADD(DAY,2,@Today) AS datetime2)),110000.00,1);
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (8,8,1,DATEADD(SECOND,64800,CAST(DATEADD(DAY,2,@Today) AS datetime2)),DATEADD(SECOND,71040,CAST(DATEADD(DAY,2,@Today) AS datetime2)),115000.00,1);
INSERT INTO dbo.[Showtimes] ([Id],[MovieId],[RoomId],[StartTime],[EndTime],[BasePrice],[IsActive]) VALUES (9,1,1,DATEADD(SECOND,75600,CAST(DATEADD(DAY,1,@Today) AS datetime2)),DATEADD(SECOND,81000,CAST(DATEADD(DAY,1,@Today) AS datetime2)),80000.00,0);
SET IDENTITY_INSERT dbo.[Showtimes] OFF;
SET IDENTITY_INSERT dbo.[Bookings] ON;
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (1,N'5548A79DFB5F480AAB53626E0940B41A',N'bbe2a506-433c-47cd-bcb5-7aba2faee1f0',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,1);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (2,N'8B23FD64F9284305B32843CE3EBED3E1',N'ce615838-6c01-4b6a-81e0-88eb8618bdbc',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,0);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (3,N'3E3BC43E118743AF8446708109DAB56F',N'bbe2a506-433c-47cd-bcb5-7aba2faee1f0',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,0);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (4,N'3D45F64E58FA493EBB60DF1554CA9D3C',N'ce615838-6c01-4b6a-81e0-88eb8618bdbc',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,1);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (5,N'FF7E78BB9613445CB999E148B016339F',N'bbe2a506-433c-47cd-bcb5-7aba2faee1f0',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,0);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (6,N'DFDC1CD6734B4FFCBE54C83531287E48',N'ce615838-6c01-4b6a-81e0-88eb8618bdbc',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,0);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (7,N'4F245EA1D75F424A9A245F3494955A2D',N'bbe2a506-433c-47cd-bcb5-7aba2faee1f0',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,1);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (8,N'489F99A801C34F9AAD6F8CB9FFA9339B',N'ce615838-6c01-4b6a-81e0-88eb8618bdbc',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,0);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (9,N'69DC1A9D1FF9476BBD585BA231846E9B',N'bbe2a506-433c-47cd-bcb5-7aba2faee1f0',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,0);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (10,N'97EB86BAB8994494831297957B6A679A',N'ce615838-6c01-4b6a-81e0-88eb8618bdbc',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,1);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (11,N'B8CF0155676B433B9239713793920996',N'bbe2a506-433c-47cd-bcb5-7aba2faee1f0',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,0);
INSERT INTO dbo.[Bookings] ([Id],[BookingCode],[UserId],[ShowtimeId],[CreatedAtUtc],[TotalAmount],[PaymentStatus]) VALUES (12,N'7A8E236ADB4A41029DD8D0E03A747101',N'ce615838-6c01-4b6a-81e0-88eb8618bdbc',1,TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),80000.00,0);
SET IDENTITY_INSERT dbo.[Bookings] OFF;
SET IDENTITY_INSERT dbo.[BookingSeats] ON;
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (1,1,1,1,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (2,2,1,2,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (3,3,1,3,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (4,4,1,4,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (5,5,1,5,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (6,6,1,6,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (7,7,1,7,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (8,8,1,8,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (9,9,1,9,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (10,10,1,10,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (11,11,1,11,80000.00);
INSERT INTO dbo.[BookingSeats] ([Id],[BookingId],[ShowtimeId],[SeatId],[UnitPrice]) VALUES (12,12,1,12,80000.00);
SET IDENTITY_INSERT dbo.[BookingSeats] OFF;
INSERT INTO dbo.[AspNetUserRoles] ([UserId],[RoleId]) VALUES (N'bbe2a506-433c-47cd-bcb5-7aba2faee1f0',N'2774726c-b2c8-4069-9f33-036e646e08cc');
INSERT INTO dbo.[AspNetUserRoles] ([UserId],[RoleId]) VALUES (N'ce615838-6c01-4b6a-81e0-88eb8618bdbc',N'2774726c-b2c8-4069-9f33-036e646e08cc');
INSERT INTO dbo.[AspNetUserRoles] ([UserId],[RoleId]) VALUES (N'91e2ccd2-d6a4-4124-9e94-1213eef9f450',N'3279b9d7-bca0-4725-bd11-41f76bfa193b');
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;
GO

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