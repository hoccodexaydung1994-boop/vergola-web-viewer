# Vergola Web Viewer v1.0

Demo: `https://<tài-khoản>.github.io/vergola-web-viewer/?model=samples/Vergola_mau.glb`

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
| ViewCube góc phải trên | Bấm mặt TRÊN / TRƯỚC / PHẢI… để nhìn theo hướng đó; nút 3D = Home |
| Model Browser (trái) | Cây Category → Family → Type → đối tượng, hoặc theo Tầng; con mắt ẩn / hiện; ô Tìm (Ctrl+F) theo tên, Type, Element ID, parameter |
| Properties (phải) | Element ID (Copy), UniqueId, Category, Family, Type, Level, Link + parameter; ô lọc |
| Chuột phải | Cô lập (I) · Ẩn (H) · Zoom vào (F) · Copy Element ID (Ctrl+C) · Bỏ chọn (Esc) · Hiện tất cả (Shift+H) · Nhìn toàn bộ (Home) |
| Cài đặt | Nền sáng / tối · viền cạnh · lọc theo tầng · góc nhìn đã lưu (trên trình duyệt này) |
| Copy link góc nhìn | Link kèm camera + ẩn / cô lập + mặt cắt (`#v=…`) |

Chuột: trái xoay · phải / giữa kéo = pan · lăn = zoom · bấm = chọn (Ctrl = chọn thêm) · bấm đúp = zoom vào.

## Định dạng GLB của Vergola
- Mét, Y lên trên (Revit feet, Z-up đã đổi: (x, y, z) → (x, z, −y)); gốc dời về tâm mô hình, đáy = 0 (toạ độ gốc ở `scene.extras.vergola.origin`).
- Mỗi đối tượng Revit = 1 node, `extras = { elementId, uniqueId, category, family, type, level, link, params:{…} }`.
- Đối tượng có hình giống hệt dùng chung 1 mesh (khác ma trận node) → file nhỏ.
- GLB khác (không có `elementId`) cũng mở được: mỗi mesh là 1 đối tượng.
- Nén thêm (tuỳ chọn, ngoài Revit): `gltfpack -i in.glb -o out.glb -cc` (meshopt) — viewer giải nén sẵn.

## Đưa lên GitHub Pages
1. Tạo repo mới (vd `vergola-web-viewer`), chép toàn bộ thư mục này vào, rồi:
   ```
   git init
   git add .
   git commit -m "Vergola Web Viewer v1.0"
   git branch -M main
   git remote add origin https://github.com/<tài-khoản>/vergola-web-viewer.git
   git push -u origin main
   ```
2. Repo → **Settings → Pages** → Source: *Deploy from a branch*, Branch: `main` / `(root)` → Save.
3. Sau 1–2 phút mở `https://<tài-khoản>.github.io/vergola-web-viewer/`.
4. Thêm mô hình: đặt `.glb` vào repo (vd `samples/`), mở `…/?model=samples/ten-file.glb`.

Lưu ý: GitHub Pages là **công khai** với repo public. Mô hình dự án thật đặt trong `models/` (đã có trong `.gitignore`) hoặc dùng repo private + Pages có kiểm soát truy cập. File > 100 MB GitHub không nhận (dùng Git LFS hoặc nén `gltfpack`).

## Cấu trúc
```
index.html            viewer 1 file (three.js 0.160 từ jsDelivr)
samples/              GLB mẫu
docs/QUY-TAC-WEB-REVIT.md   hợp đồng GLB giữa Revit và web (W1–W15)
.nojekyll             để Pages phục vụ nguyên file, không chạy Jekyll
```

Viewer gốc nằm trong Vergola Revit Suite (`web/vergola-viewer/index.html`, nhúng vào DLL). Sửa ở Suite rồi chép sang repo này, hoặc ngược lại — giữ hai bản giống nhau.
