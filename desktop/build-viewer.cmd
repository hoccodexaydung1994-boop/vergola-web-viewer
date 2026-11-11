@echo off
rem =====================================================================================
rem  BUILD + CAI VERGOLA MODEL VIEWER
rem    build-viewer.cmd            -> desktop\out\VergolaModelViewer\  (can .NET 8 Desktop Runtime tren may chay)
rem    build-viewer.cmd portable   -> desktop\out\VergolaModelViewer-portable\  (kem .NET, ~150 MB, chep sang may khac chay luon)
rem  Can: .NET 8 SDK (dotnet --list-sdks co 8.x). Lan build dau can mang de tai WebView2 (NuGet) + three.js (lib\).
rem =====================================================================================
setlocal
set HERE=%~dp0
set PROJ=%HERE%VergolaModelViewer\VergolaModelViewer.csproj
where dotnet >nul 2>nul
if errorlevel 1 (
  echo [LOI] Khong thay lenh dotnet - cai .NET 8 SDK hoac build bang Visual Studio ^(VergolaModelViewer.sln^).
  goto end
)
if /i "%~1"=="portable" goto portable

set OUT=%HERE%out\VergolaModelViewer
if exist "%OUT%" rmdir /s /q "%OUT%"
dotnet publish "%PROJ%" -c Release -o "%OUT%" --nologo
if errorlevel 1 (
  echo [LOI] Build that bai - xem loi o tren.
  goto end
)
echo.
echo Build xong: %OUT%
call "%HERE%install-viewer.cmd"
goto end

:portable
set OUT=%HERE%out\VergolaModelViewer-portable
if exist "%OUT%" rmdir /s /q "%OUT%"
dotnet publish "%PROJ%" -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=false -o "%OUT%" --nologo
if errorlevel 1 (
  echo [LOI] Build that bai - xem loi o tren.
  goto end
)
echo.
echo Build xong ban portable: %OUT%
echo Nen thu muc nay thanh zip, gui may khac: giai nen, bam dup VergolaModelViewer.exe ^(hoac install-viewer.cmd^).
pause

:end
endlocal
