@echo off
setlocal
cd /d "%~dp0"
where sqlcmd >nul 2>nul
if errorlevel 1 (
  echo Chua co sqlcmd. Hay chay START_DEMO.cmd de EF Core tu tao database.
  pause
  exit /b 1
)
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -f 65001 -i "CinemaBooking_Full.sql"
set "DB_EXIT=%ERRORLEVEL%"
if "%DB_EXIT%"=="0" (echo DATABASE_OK) else (echo Cai database that bai. Xem loi o tren.)
pause
exit /b %DB_EXIT%
