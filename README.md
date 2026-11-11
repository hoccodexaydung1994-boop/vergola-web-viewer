# Vergola Web Viewer v1.2.1

Xem mô hình Revit (GLB xuất từ Vergola Suite → **Mô hình → Xuất 3D Web (GLB)**) trên trình duyệt. Một file `index.html`, three.js 0.160 nạp từ jsDelivr (cần Internet lần đầu).

## Mở mô hình
- **Trang .html Vergola xuất kèm** (nhúng sẵn mô hình): bấm đúp là xem, gửi 1 file cho người khác.
- `index.html` → kéo thả file `.glb` vào trang, hoặc nút **Mở GLB…**.
- Trên web server (IIS, GitHub Pages…): đặt `index.html` + các `.glb` cùng thư mục, mở `index.html?model=ten-file.glb`.
  Mở bằng `file://` thì trình duyệt chặn tải file khác → dùng kéo thả hoặc trang nhúng.
- Thử ngay: `samples/Vergola_mau.glb` (45 đối tượng: cột UC dùng chung lưới, dầm, sàn, kính).

## Giao diện (khuôn Autodesk APS Viewer)
| Vùng | Có gì |
|---|---|
| Thanh công cụ dưới đáy | Home · Zoom vào chọn · Đi bộ (WASD, Q/E) · Mặt cắt / hộp cắt · Đo · Tô màu theo thuộc tính · Model Browser · Properties · Cài đặt · Toàn màn hình |
| ViewCube góc phải trên | Bấm mặt TRÊN / TRƯỚC / PHẢI… để nhìn theo hướng đó; **kéo ViewCube = xoay** quanh đối tượng đang chọn / tâm mô hình; nút 3D = Home |
| Model Browser (trái) | Cây Category → Family → Type → đối tượng, hoặc theo Tầng; con mắt ẩn / hiện; ô Tìm (Ctrl+F) theo tên, Type, Element ID, parameter |
| Properties (phải) | Element ID (Copy), UniqueId, Category, Family, Type, Level, Link + parameter; ô lọc |
| Chuột phải | Cô lập (HI) · Cô lập Category (HC) · Ẩn (HH) · Zoom vào (F) · Copy Element ID (Ctrl+C) · Bỏ chọn (Esc) · Hiện tất cả (HR) · Zoom vừa màn hình (ZF) · Góc nhìn 3D (Home) |
| Cài đặt | Nền sáng / tối · viền cạnh · lọc theo tầng · góc nhìn đã lưu (trên trình duyệt này) |
| Copy link góc nhìn | Link kèm camera + ẩn / cô lập + mặt cắt (`#v=…`) |

## Chuột & phím — giống View 3D của Revit
| Thao tác | Kết quả |
|---|---|
| Lăn chuột | Zoom **tại vị trí con trỏ** (lăn lên = lại gần) |
| Giữ chuột giữa + kéo | Pan (điểm dưới con trỏ đi theo chuột) |
| **Shift + giữ chuột giữa** + kéo | Xoay quanh **đối tượng đang chọn**; không chọn gì → quanh điểm trên mô hình dưới con trỏ (chấm xanh = tâm xoay) |
| Ctrl + Shift + chuột trái + kéo | Xoay như trên (khi không tiện chuột giữa / touchpad) |
| Bấm đúp chuột giữa | Zoom vừa màn hình (giữ hướng nhìn) |
| Bấm trái | Chọn · Ctrl + bấm = thêm · Shift + bấm = bớt · bấm chỗ trống = bỏ chọn |
| Kéo chuột trái | Chọn khung: trái → phải = đối tượng nằm trọn (khung liền) · phải → trái = chạm khung (khung nét đứt) |
| Bấm đúp trái | Zoom vào đối tượng |
| Phím | HH ẩn · HI cô lập · HC cô lập category · HR hiện lại · ZF / ZE / ZA vừa màn hình · F zoom vào chọn · Esc bỏ chọn |
| Đi bộ | W A S D / Q E; kéo chuột trái để nhìn |

## Định dạng GLB của Vergola
- Mét, Y lên trên (Revit feet, Z-up đã đổi: (x, y, z) → (x, z, −y)); gốc dời về tâm mô hình, đáy = 0 (toạ độ gốc ở `scene.extras.vergola.origin`).
- Mỗi đối tượng Revit = 1 node, `extras = { elementId, uniqueId, category, family, type, level, link, params:{…} }`.
- Đối tượng có hình giống hệt dùng chung 1 mesh (khác ma trận node) → file nhỏ.
- GLB khác (không có `elementId`) cũng mở được: mỗi mesh là 1 đối tượng.
- Nén thêm (tuỳ chọn, ngoài Revit): `gltfpack -i in.glb -o out.glb -cc` (meshopt) — viewer giải nén sẵn.

## Xem offline không cần trình duyệt / Internet
Dùng phần mềm **Vergola Model Viewer** (thư mục `desktop/` của Suite): chạy chính trang này trong WebView2, three.js lưu sẵn trong máy. Xem `desktop/README.md`.

## Lịch sử
| Phiên bản | Thay đổi |
|---|---|
| 1.2.1 | Sửa: bấm mặt ViewCube không đổi hướng nhìn (bắt chuột quá sớm khi thêm kéo ViewCube) |
| 1.2 | Điều hướng như View 3D Revit (thay OrbitControls): zoom tại con trỏ, pan chuột giữa, Shift + chuột giữa xoay quanh đối tượng chọn / điểm dưới con trỏ, đúp chuột giữa vừa màn hình, chọn khung trái→phải / phải→trái, Ctrl thêm / Shift bớt, phím HH HI HC HR ZF, kéo ViewCube xoay (Suite v1.80) |
| 1.1 | Kéo thả / Mở cả trang `.html` Vergola (lấy GLB nhúng); thả file ra ngoài vùng xem không làm trình duyệt mở file đó |
| 1.0 | Bản đầu (Suite v1.70) |
