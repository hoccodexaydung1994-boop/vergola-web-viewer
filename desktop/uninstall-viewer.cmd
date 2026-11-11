@echo off
rem =====================================================================================
rem  GO VERGOLA MODEL VIEWER: xoa thu muc cai, loi tat, dang ky "Mo bang..." / .glb.
rem  Giu lai: danh sach gan day + goc nhin da luu (%APPDATA%\Vergola\ModelViewer, %LOCALAPPDATA%\Vergola\ModelViewer).
rem =====================================================================================
setlocal
set DEST=%LOCALAPPDATA%\Programs\Vergola\ModelViewer
set "CLS=HKCU\Software\Classes"
echo.
echo ===== GO VERGOLA MODEL VIEWER =====
taskkill /im VergolaModelViewer.exe /f >nul 2>nul
if exist "%DEST%" rmdir /s /q "%DEST%"
if exist "%DEST%" echo [LOI] Khong xoa duoc %DEST% - dong Explorer dang mo thu muc do roi chay lai.
del /q "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Vergola\Vergola Model Viewer.lnk" >nul 2>nul
rmdir "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Vergola" >nul 2>nul
powershell -NoProfile -Command "Remove-Item -LiteralPath ([Environment]::GetFolderPath('Desktop') + '\Vergola Model Viewer.lnk') -ErrorAction SilentlyContinue" >nul 2>nul

reg delete "%CLS%\Vergola.ModelViewer.glb" /f >nul 2>nul
reg delete "%CLS%\.glb\OpenWithProgids" /v "Vergola.ModelViewer.glb" /f >nul 2>nul
reg delete "%CLS%\Applications\VergolaModelViewer.exe" /f >nul 2>nul
reg delete "%CLS%\.html\OpenWithList\VergolaModelViewer.exe" /f >nul 2>nul
for /f "tokens=3" %%V in ('reg query "%CLS%\.glb" /ve 2^>nul ^| find /i "Vergola.ModelViewer.glb"') do reg delete "%CLS%\.glb" /ve /f >nul 2>nul
ie4uinit.exe -show >nul 2>nul
echo   Da go.
echo.
pause
