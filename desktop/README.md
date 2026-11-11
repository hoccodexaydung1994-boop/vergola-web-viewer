# Vergola Model Viewer 1.0

Phần mềm Windows xem mô hình 3D Vergola **không cần Revit, không cần Internet**. Mở được:
- file `.glb` (Revit → Vergola Suite → **Mô hình → Xuất 3D Web (GLB)**),
- trang `.html` Vergola xuất kèm (lấy mô hình nhúng bên trong ra).

Giao diện và công cụ giống hệt Vergola Web Viewer (thanh dưới đáy, ViewCube, Model Browser, Properties, mặt cắt, đo, tô màu…), vì phần mềm chạy chính trang `index.html` đó trong khung **Microsoft Edge WebView2** (Windows 10/11 có sẵn).

## Build + cài (1 lần)
1. Cần .NET 8 SDK (Visual Studio 2022 đã kèm). Lần build đầu cần mạng: tải gói WebView2 (NuGet) và thư viện three.js vào `VergolaModelViewer\lib\`.
2. Bấm đúp `build-viewer.cmd` → build ra `out\VergolaModelViewer\` rồi tự chạy `install-viewer.cmd`:
   - chép vào `%LOCALAPPDATA%\Programs\Vergola\ModelViewer\` (không cần quyền Admin),
   - lối tắt Start Menu → Vergola → Vergola Model Viewer, và trên Desktop,
   - chuột phải file `.glb` / `.html` → **Mở bằng (Open with)** → Vergola Model Viewer,
   - hỏi có đặt làm mặc định khi bấm đúp `.glb` không.
3. Hoặc build trong Visual Studio: mở `VergolaModelViewer.sln` → Build → bấm đúp `install-viewer.cmd` trong thư mục output.

Gỡ: `uninstall-viewer.cmd`.

### Gửi cho máy không có .NET
`build-viewer.cmd portable` → `out\VergolaModelViewer-portable\` (kèm .NET, ~150 MB). Nén zip gửi đi: giải nén, bấm đúp `VergolaModelViewer.exe` (hoặc `install-viewer.cmd` để có lối tắt + Mở bằng).

## Dùng
- **Mở mô hình…** (Ctrl+O) · **Gần đây ▾** (10 file mở gần nhất) · **Mở thư mục** · **Mô hình mẫu**.
- Kéo thả file `.glb` / `.html` vào cửa sổ.
- Dòng lệnh: `VergolaModelViewer.exe "D:\Du an\Nha A.glb"`.
- Góc chữ phải thanh trên: **Offline · three.js 0.160** = đã đủ thư viện, chạy không cần mạng.

## Cách chạy offline
| Trang yêu cầu | Phần mềm trả |
|---|---|
| `https://vergola.local/index.html` | `web\index.html` cạnh .exe |
| `https://cdn.jsdelivr.net/npm/three@0.160.0/…` | `lib\three@0.160.0\…` cạnh .exe — không ra mạng. File nào thiếu: tải 1 lần khi có mạng rồi lưu lại |
| `https://model.vergola.local/…` | file `.glb` đang mở, đọc thẳng từ đĩa (không chép) |

Nhờ vậy cả trang `.html` cũ (importmap trỏ CDN) cũng chạy offline trong phần mềm này. Mô hình lấy từ `.html` được giải ra `%TEMP%\Vergola\ModelViewer\` và tự xoá sau 2 ngày.

## Cấu trúc
```
desktop/
├─ Directory.Build.props        ModelViewerRelease (số phiên bản), chặn cấu hình Revit của Suite
├─ VergolaModelViewer.sln
├─ build-viewer.cmd             build + cài; "portable" = kèm .NET
├─ install-viewer.cmd / uninstall-viewer.cmd
└─ VergolaModelViewer/
   ├─ VergolaModelViewer.csproj  WPF net8.0-windows + Microsoft.Web.WebView2; chép web\ và lib\ ra output
   ├─ App.xaml(.cs)              đọc tham số dòng lệnh
   ├─ MainWindow.xaml(.cs)       thanh trên + WebView2, phục vụ trang / three.js / mô hình
   ├─ ModelSource.cs             .glb → kiểm; .html → lấy GLB nhúng; danh sách gần đây
   └─ Assets/app.ico
```
Trang viewer lấy từ `web/vergola-viewer/index.html` (repo Suite) hoặc `index.html` ở gốc (repo GitHub `vergola-web-viewer`) — sửa viewer 1 chỗ, phần mềm build lại là có.

## Giới hạn
- Chỉ Windows 10/11 (WebView2). Máy thiếu WebView2 Runtime: phần mềm hiện nút tải bản cài của Microsoft.
- File `.gltf` nhiều file rời chưa hỗ trợ (Vergola chỉ xuất `.glb`).
- "Copy link góc nhìn" trong phần mềm tạo link nội bộ, chỉ dùng được trong phần mềm; gửi người khác thì gửi file `.html`.
