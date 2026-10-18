using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var project = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../CinemaBooking"));
var appDll = Path.Combine(project, "bin/Debug/net10.0/CinemaBooking.dll");
var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("CINEMA_TEST_CONNECTION")
    ?? "Server=(localdb)\\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False");
connection.InitialCatalog = "CinemaBooking_Smoke_" + Guid.NewGuid().ToString("N")[..12];
var password = "Qa!" + Guid.NewGuid().ToString("N");
var services = new ServiceCollection();
services.AddLogging();
services.AddDataProtection();
services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(
    new Dictionary<string, string?> { ["DemoData:Password"] = password }).Build());
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connection.ConnectionString));
services.AddIdentityCore<IdentityUser>(options => options.Stores.MaxLengthForKeys = 128).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
await using var provider = services.BuildServiceProvider();
var checks = new List<object>();
Process? server = null;
var serverLog = new System.Text.StringBuilder();
void Check(string name, bool passed)
{
    checks.Add(new { name, passed });
    Console.WriteLine($"{(passed ? "PASS" : "FAIL")} {name}");
    if (!passed) throw new InvalidOperationException(name);
}
async Task<T> WithDb<T>(Func<ApplicationDbContext, Task<T>> action)
{
    using var scope = provider.CreateScope();
    return await action(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
}
try
{
    using (var scope = provider.CreateScope())
    {
        await DemoDataSeeder.SeedAsync(scope.ServiceProvider);
    }
    var counts = await WithDb(async db => new[] { await db.Movies.CountAsync(), await db.Showtimes.CountAsync(), await db.Bookings.CountAsync() });
    using (var scope = provider.CreateScope())
    {
        await DemoDataSeeder.SeedAsync(scope.ServiceProvider);
    }
    var countsAfter = await WithDb(async db => new[] { await db.Movies.CountAsync(), await db.Showtimes.CountAsync(), await db.Bookings.CountAsync() });
    Check("Migration và demo seed chạy lại không tạo trùng", counts.SequenceEqual(countsAfter));
    Check("Áp dụng đầy đủ migration", (await WithDb(async db => (await db.Database.GetPendingMigrationsAsync()).Count())) == 0);

    var fixture = await WithDb(async db =>
    {
        var movie = new Movie { Title = "QA Regression", Description = "Regression", DurationMinutes = 60,
            ReleaseDate = DateTime.Today, IsActive = true, PosterUrl = "/images/posters/demo-01.jpg" };
        var room = new Room { Name = "QA Regression", IsActive = true };
        for (var i = 1; i <= 60; i++) room.Seats.Add(new Seat { RowLabel = "A", SeatNumber = i, IsActive = true });
        db.AddRange(movie, room);
        await db.SaveChangesAsync();
        var shows = Enumerable.Range(0, 6).Select(i => new Showtime { MovieId = movie.Id, RoomId = room.Id,
            StartTime = DateTime.Today.AddDays(10).AddHours(9 + i * 2), EndTime = DateTime.Today.AddDays(10).AddHours(10 + i * 2),
            BasePrice = 80000, IsActive = true }).ToList();
        db.Showtimes.AddRange(shows);
        await db.SaveChangesAsync();
        return (MovieId: movie.Id, RoomId: room.Id, Shows: shows.Select(s => s.Id).ToArray(), Seats: room.Seats.OrderBy(s => s.SeatNumber).Select(s => s.Id).ToArray());
    });
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start(); var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
    var baseUrl = $"http://127.0.0.1:{port}";
    var start = new ProcessStartInfo("dotnet") { WorkingDirectory = project, UseShellExecute = false,
        CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
    start.ArgumentList.Add(appDll);
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
    start.Environment["ASPNETCORE_URLS"] = baseUrl;
    start.Environment["ConnectionStrings__DefaultConnection"] = connection.ConnectionString;
    start.Environment["Logging__EventLog__LogLevel__Default"] = "None";
    server = Process.Start(start)!;
    server.OutputDataReceived += (_, e) => { if (e.Data != null) lock (serverLog) serverLog.AppendLine(e.Data); };
    server.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (serverLog) serverLog.AppendLine(e.Data); };
    server.BeginOutputReadLine(); server.BeginErrorReadLine();
    HttpClient Client() => new(new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false, CookieContainer = new CookieContainer() })
        { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(45) };
    using var guest = Client();
    for (var i = 0; ; i++)
    {
        try { using var response = await guest.GetAsync("/Identity/Account/Login"); if (response.IsSuccessStatusCode) break; }
        catch (HttpRequestException) when (i < 60) { }
        if (i >= 60) throw new InvalidOperationException("Server chưa khởi động.");
        await Task.Delay(250);
    }
    async Task<string> Html(HttpClient client, string path) => await client.GetStringAsync(path);
    string Token(string html)
    {
        var match = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.IgnoreCase);
        if (!match.Success) throw new InvalidOperationException("Không tìm thấy antiforgery token.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
    async Task Login(HttpClient client, string email)
    {
        var html = await Html(client, "/Identity/Account/Login");
        var response = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string> {
            ["Input.Email"] = email, ["Input.Password"] = password, ["__RequestVerificationToken"] = Token(html) }));
        Check("Đăng nhập " + email, response.StatusCode == HttpStatusCode.Redirect);
    }
    using var a = Client(); using var b = Client(); using var admin = Client(); using var admin2 = Client();
    await Login(a, "demo.a@example.com"); await Login(b, "demo.b@example.com");
    await Login(admin, "demo.admin@example.com"); await Login(admin2, "demo.admin@example.com");

    var badDate = await guest.GetAsync($"/Catalog/Details/{fixture.MovieId}?date=abc");
    Check("Ngày sai trả 400 có layout", badDate.StatusCode == HttpStatusCode.BadRequest && (await badDate.Content.ReadAsStringAsync()).Contains("main-content"));
    var missing = await guest.GetAsync("/missing-regression-page");
    Check("URL không tồn tại trả 404 có layout", missing.StatusCode == HttpStatusCode.NotFound && (await missing.Content.ReadAsStringAsync()).Contains("main-content"));
    var unauth = await guest.GetAsync($"/api/showtimes/{fixture.Shows[0]}/selection");
    Check("API chưa đăng nhập trả 401", unauth.StatusCode == HttpStatusCode.Unauthorized);
    var apiMissing = await a.GetAsync("/api/showtimes/2147483647/seats");
    Check("API không bị đổi thành HTML trang lỗi", apiMissing.StatusCode == HttpStatusCode.NotFound && apiMissing.Content.Headers.ContentType?.MediaType == "application/json");
    var guestAdmin = await guest.GetAsync("/Rooms");
    Check("Khách chưa đăng nhập bị yêu cầu đăng nhập", guestAdmin.StatusCode == HttpStatusCode.Redirect && guestAdmin.Headers.Location!.ToString().Contains("Login"));
    var userAdmin = await a.GetAsync("/Rooms");
    Check("Khách thường không mở được quản trị", userAdmin.StatusCode == HttpStatusCode.Redirect && userAdmin.Headers.Location!.ToString().Contains("AccessDenied"));

    var genreId = await WithDb(db => db.Genres.Select(g => g.Id).FirstAsync());
    var posterError = await admin.PostAsync("/Movies/Create", new FormUrlEncodedContent(new Dictionary<string, string> {
        ["Title"] = "QA Poster Invalid", ["Description"] = "QA", ["DurationMinutes"] = "60",
        ["ReleaseDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["PosterUrl"] = new string('a', 501),
        ["IsActive"] = "true", ["selectedGenreIds"] = genreId.ToString(),
        ["__RequestVerificationToken"] = Token(await Html(admin, "/Movies/Create")) }));
    Check("Poster 501 ký tự báo lỗi tiếng Việt", posterError.IsSuccessStatusCode && WebUtility.HtmlDecode(await posterError.Content.ReadAsStringAsync()).Contains("Đường dẫn ảnh poster tối đa 500 ký tự."));
    Check("Poster không hợp lệ không tạo phim", !await WithDb(db => db.Movies.AnyAsync(m => m.Title == "QA Poster Invalid")));

    var noTokenPayment = await admin.PostAsync("/AdminBookings/ConfirmPayment/1", new FormUrlEncodedContent(new Dictionary<string, string>()));
    Check("POST quản trị thiếu antiforgery bị chặn", noTokenPayment.StatusCode == HttpStatusCode.BadRequest);

    async Task<HttpResponseMessage> Selection(HttpClient client, string json, bool token = true)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"/api/showtimes/{fixture.Shows[0]}/selection")
            { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
        if (token) req.Headers.Add("RequestVerificationToken", Token(await Html(client, $"/Bookings/Create?showtimeId={fixture.Shows[0]}")));
        return await client.SendAsync(req);
    }
    foreach (var json in new[] { "{}", "{\"seatIds\":null}", "{\"seatIds\":[-1]}", "{\"seatIds\":[0]}", "{\"seatIds\":[1,1]}", "{", "{\"seatIds\":[1,2,3,4,5,6,7,8,9,10,11]}" })
    {
        using var response = await Selection(a, json);
        Check("Chặn JSON selection " + json, response.StatusCode == HttpStatusCode.BadRequest);
    }
    Check("POST selection thiếu token bị chặn", (await Selection(a, "{\"seatIds\":[]}", false)).StatusCode == HttpStatusCode.BadRequest);
    var savedSeat = fixture.Seats[50];
    Check("Lưu lựa chọn hợp lệ", (await Selection(a, $"{{\"seatIds\":[{savedSeat}]}}")).IsSuccessStatusCode);
    async Task<JsonElement> ReadSelection(HttpClient client) => (await client.GetFromJsonAsync<JsonElement>($"/api/showtimes/{fixture.Shows[0]}/selection"));
    Check("Khôi phục đúng lựa chọn", (await ReadSelection(a)).GetProperty("seatIds")[0].GetInt32() == savedSeat);
    Check("Lựa chọn không lộ sang tài khoản B", (await ReadSelection(b)).GetProperty("seatIds").GetArrayLength() == 0);
    Check("Body thiếu trường không xóa lựa chọn cũ", (await Selection(a, "{}")).StatusCode == HttpStatusCode.BadRequest && (await ReadSelection(a)).GetProperty("seatIds").GetArrayLength() == 1);
    await WithDb(async db => { await db.Seats.Where(s => s.Id == savedSeat).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false)); return 0; });
    Check("Khôi phục loại ghế bị tắt", (await ReadSelection(a)).GetProperty("removedSeatIds")[0].GetInt32() == savedSeat);
    await WithDb(async db => { await db.Seats.Where(s => s.Id == savedSeat).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, true)); return 0; });
    Check("Danh sách rỗng xóa lựa chọn", (await Selection(a, "{\"seatIds\":[]}")).IsSuccessStatusCode && (await ReadSelection(a)).GetProperty("seatIds").GetArrayLength() == 0);

    async Task<HttpResponseMessage> Book(HttpClient client, int show, int[] seats)
    {
        var token = Token(await Html(client, $"/Bookings/Create?showtimeId={show}"));
        var values = new List<KeyValuePair<string, string>> { new("ShowtimeId", show.ToString()), new("__RequestVerificationToken", token), new("TotalAmount", "1") };
        values.AddRange(seats.Select(id => new KeyValuePair<string, string>("SelectedSeatIds", id.ToString())));
        return await client.PostAsync("/Bookings/Create", new FormUrlEncodedContent(values));
    }
    bool Success(HttpResponseMessage response) => response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location?.ToString().Contains("/Bookings/Details/") == true;
    for (var round = 0; round < 10; round++)
    {
        var responses = await Task.WhenAll(Book(a, fixture.Shows[0], new[] { fixture.Seats[round * 2] }), Book(b, fixture.Shows[0], new[] { fixture.Seats[round * 2 + 1] }));
        Check($"Ghế khác nhau cùng suất lượt {round + 1}", responses.All(Success));
    }
    var same = await Task.WhenAll(Book(a, fixture.Shows[0], new[] { fixture.Seats[25] }), Book(b, fixture.Shows[0], new[] { fixture.Seats[25] }));
    Check("Cùng ghế chỉ một đơn thành công", same.Count(Success) == 1);
    var otherShows = await Task.WhenAll(Book(a, fixture.Shows[1], new[] { fixture.Seats[26] }), Book(b, fixture.Shows[2], new[] { fixture.Seats[26] }));
    Check("Cùng ghế vật lý ở hai suất đều đặt được", otherShows.All(Success));
    await Selection(a, $"{{\"seatIds\":[{fixture.Seats[27]}]}}");
    var booking = await Book(a, fixture.Shows[0], new[] { fixture.Seats[27] });
    Check("Đặt vé hợp lệ", Success(booking));
    var detailUrl = booking.Headers.Location!.ToString();
    var bookingId = int.Parse(detailUrl.Split('/').Last());
    Check("Giá lấy từ server", await WithDb(async db => (await db.Bookings.FindAsync(bookingId))!.TotalAmount) == 80000);
    Check("Đặt xong xóa session", (await ReadSelection(a)).GetProperty("seatIds").GetArrayLength() == 0);
    Check("Ngày đặt không in mã CSharp", !(await Html(a, detailUrl)).Contains(".ToString("));
    Check("B không xem được đơn của A", (await b.GetAsync(detailUrl)).StatusCode == HttpStatusCode.NotFound);

    var adminToken = Token(await Html(admin, "/AdminBookings"));
    async Task<HttpResponseMessage> Pay(HttpClient client, string token) => await client.PostAsync($"/AdminBookings/ConfirmPayment/{bookingId}",
        new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));
    var userPayment = await Pay(a, Token(await Html(a, $"/Bookings/Create?showtimeId={fixture.Shows[0]}")));
    Check("Khách không xác nhận được thanh toán", userPayment.StatusCode == HttpStatusCode.Redirect && userPayment.Headers.Location!.ToString().Contains("AccessDenied"));
    var paid = await Task.WhenAll(Pay(admin, adminToken), Pay(admin2, Token(await Html(admin2, "/AdminBookings"))));
    Check("Hai Admin xác nhận cùng đơn an toàn", paid.All(r => r.StatusCode == HttpStatusCode.Redirect) && await WithDb(async db => (await db.Bookings.FindAsync(bookingId))!.PaymentStatus) == BookingPaymentStatus.Paid);
    Check("Xác nhận lại không tạo đơn mới", (await Pay(admin, adminToken)).StatusCode == HttpStatusCode.Redirect && await WithDb(db => db.Bookings.CountAsync(x => x.Id == bookingId)) == 1);

    var openForm = Token(await Html(a, $"/Bookings/Create?showtimeId={fixture.Shows[3]}"));
    var saleValues = new Dictionary<string, string> { ["isActive"] = "false", ["__RequestVerificationToken"] = Token(await Html(admin, "/Showtimes")) };
    Check("Admin đóng bán", (await admin.PostAsync($"/Showtimes/SetSale/{fixture.Shows[3]}", new FormUrlEncodedContent(saleValues))).StatusCode == HttpStatusCode.Redirect);
    Check("Đóng bán lặp không đảo lại", (await admin.PostAsync($"/Showtimes/SetSale/{fixture.Shows[3]}", new FormUrlEncodedContent(saleValues))).StatusCode == HttpStatusCode.Redirect && !await WithDb(async db => (await db.Showtimes.FindAsync(fixture.Shows[3]))!.IsActive));
    var closedPost = await a.PostAsync("/Bookings/Create", new FormUrlEncodedContent(new Dictionary<string, string> { ["ShowtimeId"] = fixture.Shows[3].ToString(), ["SelectedSeatIds"] = fixture.Seats[30].ToString(), ["__RequestVerificationToken"] = openForm }));
    Check("Suất đóng bán chuyển về Catalog", closedPost.StatusCode == HttpStatusCode.Redirect && closedPost.Headers.Location!.ToString().Contains("/Catalog"));
    var closedHtml = WebUtility.HtmlDecode(await Html(a, closedPost.Headers.Location!.ToString()));
    Check("Thông báo lỗi đặt vé hiện trên Catalog", closedHtml.Contains("Suất chiếu không còn đủ điều kiện đặt vé") && closedHtml.Contains("role=\"alert\""));
    Check("Suất đóng bán không mở form đặt vé", (await a.GetAsync($"/Bookings/Create?showtimeId={fixture.Shows[3]}")).StatusCode == HttpStatusCode.NotFound);

    var usedEdit = await admin.GetAsync($"/Showtimes/Edit/{fixture.Shows[0]}");
    Check("Không sửa suất có đơn", usedEdit.StatusCode == HttpStatusCode.Redirect);
    var deleteUsed = await admin.PostAsync($"/Showtimes/Delete/{fixture.Shows[0]}", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = Token(await Html(admin, "/Showtimes")) }));
    Check("Không xóa suất có đơn", deleteUsed.StatusCode == HttpStatusCode.Redirect && await WithDb(db => db.Showtimes.AnyAsync(s => s.Id == fixture.Shows[0])));
    Check("Không có ghế trùng trong database", !await WithDb(db => db.BookingSeats.GroupBy(s => new { s.ShowtimeId, s.SeatId }).AnyAsync(g => g.Count() > 1)));

    await WithDb(async db => { await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER dbo.SmokeFailure ON dbo.BookingSeats AFTER INSERT AS BEGIN THROW 51000, 'QA rollback', 1; END"); return 0; });
    var before = await WithDb(db => db.Bookings.CountAsync());
    var failedBooking = await Book(a, fixture.Shows[4], new[] { fixture.Seats[39], fixture.Seats[40] });
    Check("Lỗi ghi giữa giao dịch rollback toàn bộ", !Success(failedBooking) && await WithDb(db => db.Bookings.CountAsync()) == before && !await WithDb(db => db.BookingSeats.AnyAsync(s => s.ShowtimeId == fixture.Shows[4])));
    await WithDb(async db => { await db.Database.ExecuteSqlRawAsync("DROP TRIGGER dbo.SmokeFailure"); return 0; });

    var poster = await guest.GetAsync("/images/movies/mai.jpg");
    Check("Poster dữ liệu cũ trả ảnh JPEG", poster.IsSuccessStatusCode && poster.Content.Headers.ContentType?.MediaType == "image/jpeg");
    var htmlCatalog = await Html(guest, "/Catalog");
    Check("Catalog có poster và bộ lọc", htmlCatalog.Contains("poster") && htmlCatalog.Contains("GenreId"));
    Check("Trang tài khoản tiếng Việt", (await Html(guest, "/Identity/Account/Login")).Contains("lang=\"vi\""));

    await WithDb(async db => { await db.Seats.Where(s => s.RoomId == fixture.RoomId).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false)); return 0; });
    Check("Phòng không còn ghế hoạt động không mở đặt vé", (await a.GetAsync($"/Bookings/Create?showtimeId={fixture.Shows[5]}")).StatusCode == HttpStatusCode.NotFound);
    Console.WriteLine($"Completed {checks.Count} checks.");
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    Environment.ExitCode = 1;
    var diagnosticPath = Environment.GetEnvironmentVariable("CINEMA_TEST_LOG");
    if (!string.IsNullOrWhiteSpace(diagnosticPath)) File.WriteAllText(diagnosticPath, serverLog.ToString());
}
finally
{
    if (server is { HasExited: false }) { server.Kill(entireProcessTree: true); await server.WaitForExitAsync(); }
    using var cleanup = provider.CreateScope();
    await cleanup.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureDeletedAsync();
    var resultsPath = Environment.GetEnvironmentVariable("CINEMA_TEST_RESULTS");
    if (!string.IsNullOrWhiteSpace(resultsPath)) File.WriteAllText(resultsPath, JsonSerializer.Serialize(checks, new JsonSerializerOptions { WriteIndented = true }));
}
