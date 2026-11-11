@echo off
rem =====================================================================================
rem  CAI VERGOLA MODEL VIEWER (xem mo hinh GLB / trang .html Vergola, KHONG can Revit, KHONG can Internet)
rem  Bam dup file nay sau khi da build. Tu tim VergolaModelViewer.exe:
rem    a) nam canh file nay (thu muc output, vd ...\bin\Release\net8.0-windows\ hoac desktop\out\VergolaModelViewer\)
rem    b) hoac trong desktop\out\VergolaModelViewer\ / desktop\VergolaModelViewer\bin\(Release^|Debug)\... (ban moi nhat)
rem  Chep vao %LOCALAPPDATA%\Programs\Vergola\ModelViewer\ (khong can quyen Admin)
rem  + loi tat Start Menu / Desktop + "Mo bang..." cho .glb va .html (+ tuy chon: mac dinh cho .glb)
rem  Go: uninstall-viewer.cmd
rem =====================================================================================
setlocal EnableDelayedExpansion
set HERE=%~dp0
set DEST=%LOCALAPPDATA%\Programs\Vergola\ModelViewer
set EXE=%DEST%\VergolaModelViewer.exe
set SRC=
echo.
echo ===== CAI VERGOLA MODEL VIEWER =====
echo.

:checkrun
tasklist /fi "imagename eq VergolaModelViewer.exe" 2>nul | find /i "VergolaModelViewer.exe" >nul
if not errorlevel 1 (
  echo [CHU Y] Vergola Model Viewer dang mo - dong het cua so roi bam phim bat ky ^(Ctrl+C de thoat^).
  pause >nul
  goto checkrun
)

if exist "%HERE%VergolaModelViewer.exe" set "SRC=%HERE%"
if defined SRC goto havesrc
if exist "%HERE%out\VergolaModelViewer\VergolaModelViewer.exe" set "SRC=%HERE%out\VergolaModelViewer\"
if defined SRC goto havesrc
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "$f = Get-ChildItem -Path '%HERE%VergolaModelViewer\bin' -Recurse -Filter VergolaModelViewer.exe -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1; if ($f) { $f.DirectoryName + '\' }"`) do set "SRC=%%D"
if defined SRC goto havesrc
echo [LOI] Chua thay VergolaModelViewer.exe.
echo       Chay build-viewer.cmd ^(hoac Visual Studio: mo VergolaModelViewer.sln, Build ^> Rebuild^) roi chay lai file nay.
goto done

:havesrc
echo   Nguon : !SRC!
if not exist "!SRC!web\index.html" (
  echo [LOI] Thieu web\index.html canh VergolaModelViewer.exe - build lai.
  goto done
)
if not exist "!SRC!lib\three@0.160.0\build\three.module.js" echo   [CHU Y] Chua co thu vien three.js offline - lan dau mo phan mem can noi mang 1 lan.

if exist "%DEST%" rmdir /s /q "%DEST%"
if exist "%DEST%" (
  echo [LOI] Khong xoa duoc %DEST% ^(file dang bi khoa?^). Dong phan mem / Explorer dang mo thu muc do roi chay lai.
  goto done
)
xcopy "!SRC!*" "%DEST%\" /e /i /y /q >nul
if errorlevel 1 (
  echo [LOI] Chep file that bai.
  goto done
)
powershell -NoProfile -Command "Get-ChildItem -Path '%DEST%' -Recurse | Unblock-File -ErrorAction SilentlyContinue" >nul 2>nul

rem ---------- loi tat
set "MENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Vergola"
if not exist "%MENU%" mkdir "%MENU%"
powershell -NoProfile -Command "$w = New-Object -ComObject WScript.Shell; foreach ($p in @('%MENU%\Vergola Model Viewer.lnk', [Environment]::GetFolderPath('Desktop') + '\Vergola Model Viewer.lnk')) { $s = $w.CreateShortcut($p); $s.TargetPath = '%EXE%'; $s.WorkingDirectory = '%DEST%'; $s.IconLocation = '%EXE%,0'; $s.Description = 'Xem mo hinh 3D Vergola (GLB) khong can Revit'; $s.Save() }" >nul 2>nul

rem ---------- "Mo bang..." cho .glb / .html (chi tai khoan nay, khong can Admin)
set "CLS=HKCU\Software\Classes"
reg add "%CLS%\Vergola.ModelViewer.glb" /ve /d "Vergola 3D model (GLB)" /f >nul
reg add "%CLS%\Vergola.ModelViewer.glb\DefaultIcon" /ve /d "\"%EXE%\",0" /f >nul
reg add "%CLS%\Vergola.ModelViewer.glb\shell\open\command" /ve /d "\"%EXE%\" \"%%1\"" /f >nul
reg add "%CLS%\.glb\OpenWithProgids" /v "Vergola.ModelViewer.glb" /t REG_NONE /f >nul
reg add "%CLS%\Applications\VergolaModelViewer.exe" /v "FriendlyAppName" /d "Vergola Model Viewer" /f >nul
reg add "%CLS%\Applications\VergolaModelViewer.exe\shell\open\command" /ve /d "\"%EXE%\" \"%%1\"" /f >nul
reg add "%CLS%\Applications\VergolaModelViewer.exe\SupportedTypes" /v ".glb" /d "" /f >nul
reg add "%CLS%\Applications\VergolaModelViewer.exe\SupportedTypes" /v ".html" /d "" /f >nul
reg add "%CLS%\Applications\VergolaModelViewer.exe\SupportedTypes" /v ".htm" /d "" /f >nul
reg add "%CLS%\.html\OpenWithList\VergolaModelViewer.exe" /f >nul 2>nul

echo.
set ASSOC=N
set /p ASSOC=Dat Vergola Model Viewer lam MAC DINH khi bam dup file .glb? [Y/N, Enter = N]:
if /i "!ASSOC!"=="Y" (
  reg add "%CLS%\.glb" /ve /d "Vergola.ModelViewer.glb" /f >nul
  echo   Da dat. Neu Windows van hoi "Mo bang ung dung nao", chon Vergola Model Viewer + tick "Luon dung".
)
ie4uinit.exe -show >nul 2>nul

echo.
echo   Da cai: %EXE%
echo   Loi tat: Start Menu ^> Vergola ^> Vergola Model Viewer, va tren Desktop.
echo   File .glb / .html: chuot phai ^> Mo bang ^(Open with^) ^> Vergola Model Viewer.

:done
echo.
pause
