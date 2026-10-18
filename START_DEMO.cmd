@echo off
setlocal DisableDelayedExpansion
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Can cai .NET 10 SDK truoc khi chay.
  pause
  exit /b 1
)
set "ASPNETCORE_ENVIRONMENT=Development"
set "ConnectionStrings__DefaultConnection=Server=(localdb)\MSSQLLocalDB;Database=CinemaBooking_Submission;Trusted_Connection=True;MultipleActiveResultSets=true"
set "DemoData__Enabled=true"
set "DemoData__Password=CinemaDemo@2026!"
echo CinemaBooking - http://localhost:5175
echo Admin: demo.admin@example.com
echo Khach: demo.a@example.com hoac demo.b@example.com
echo Mat khau demo: CinemaDemo@2026!
echo Mo URL tren sau khi thay "Now listening on". Dung server bang Ctrl+C.
dotnet run --project ".\CinemaBooking\CinemaBooking.csproj" --launch-profile http
set "APP_EXIT=%ERRORLEVEL%"
if not "%APP_EXIT%"=="0" pause
exit /b %APP_EXIT%
