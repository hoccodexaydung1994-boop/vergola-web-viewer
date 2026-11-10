# Quy tắc Web–Revit (hợp đồng GLB) — schema 1

Áp dụng cho Vergola Revit Suite v1.70 (exporter) và Vergola Web Viewer v1.0. Khi mâu thuẫn, sửa bảng này trước rồi mới sửa code.

## A. Dữ liệu GLB
| Mã | Quy tắc |
|---|---|
| W1 | Mỗi lần xuất ra 1 file glTF 2.0 nhị phân (`.glb`): chunk JSON rồi chunk BIN, không texture, không file ngoài. |
| W2 | Đơn vị mét, trục Y hướng lên. Trục Revit (x, y, z) → glTF (x, z, −y); feet chia 3,28084. |
| W3 | Gốc toạ độ dời về tâm mô hình khi bật `CenterOrigin`; độ dời ghi ở `scene.extras.vergola.origin` (mét). |
| W4 | 1 element = 1 node. `node.extras` luôn có `elementId` (chuỗi), `uniqueId`, `category`, `type`; có `family`, `level` khi element có; thêm `link` nếu từ file liên kết, `params` nếu chọn Basic / All. |
| W5 | Khoá nhận dạng bền là `uniqueId` (+ `link`). `elementId` chỉ để hiển thị. |
| W6 | `scene.extras.vergola` có: `project`, `view`, `exportedAt`, `revit`, `exporter`, `units`, `upAxis`, `origin`. |
| W7 | Chỉ thêm khoá, không đổi nghĩa khoá cũ. Viewer bỏ qua khoá lạ, hiện "—" khi thiếu khoá. |

## B. Phiên bản & tương thích
| Mã | Quy tắc |
|---|---|
| W8 | Exporter chạy Revit 2022–2025 cùng 1 file code; khác biệt API qua `RevitIds`. |
| W9 | Exporter chỉ đọc, không mở Transaction; chạy qua `RevitTaskQueue`. |
| W10 | Khi vi phạm W7, thêm `schema` vào `vergola` và tăng số; viewer đọc mọi schema từ 1. Chưa có khoá = schema 1. |
| W11 | Viewer chạy Chrome, Edge, Firefox, Safari hiện hành có WebGL2; mở được từ `file://` (GLB nhúng) và web server (`?model=`). |

## C. Giao diện, hiệu năng, bảo mật
| Mã | Quy tắc |
|---|---|
| W12 | Bố cục APS Viewer: toolbar dưới giữa, cây trái, thuộc tính phải, ViewCube trên phải; màu / cỡ chữ theo token G21. |
| W13 | Chữ tiếng Việt; tên Revit (Category, Family, Type, Level) giữ như trong model. |
| W14 | Giữ ngân sách hiệu năng; danh sách DOM dài có giới hạn; không lặp theo element trong mỗi khung hình. |
| W15 | Không tải mô hình lên server nào. Chỉ tải thư viện từ CDN cố định phiên bản (three.js 0.160, jsDelivr); không analytics, không cookie. |

## Lịch sử
| Ngày | Suite | Schema | Thay đổi |
|---|---|---|---|
| 2026-11-10 | v1.70 | 1 | Bản đầu: W1–W15 |
