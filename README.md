# Lab06 Final: Secure Medical Supplies Catalog

## Bài toán: Medical Supplies Catalog — Final Secure MVC Project

Nâng cấp từ Lab05 để tích hợp toàn bộ kiến thức môn học: quản lý vật tư y tế, cấp phát kho, xác thực người dùng, phân quyền chi tiết dựa trên Policy, bảo mật nâng cao chống CSRF/XSS/SQLi, tải file hình ảnh an toàn, nhật ký kiểm toán (Audit Logs), đo lường vận hành (Observability), container hóa (Docker) và tích hợp CI/CD.

---

## 🛠️ Hướng dẫn cài đặt và chạy

### Cách 1: Chạy trực tiếp qua .NET CLI

1. **Di chuyển vào thư mục dự án**:
   ```bash
   cd Lab06
   ```
2. **Khởi chạy ứng dụng**:
   ```bash
   dotnet run --launch-profile http
   ```
   Ứng dụng sẽ tự động chạy lệnh Migration tạo cơ sở dữ liệu SQLite (`MedicalSuppliesLab06.db`), nạp dữ liệu mẫu (Seed Data) và lắng nghe tại địa chỉ: `http://localhost:5011`

---

### Cách 2: Chạy bằng Docker (Khuyến nghị)

Ứng dụng đã được cấu hình chạy đa môi trường (Production) qua Docker và lưu cơ sở dữ liệu SQLite ổn định thông qua volume mount ngoài container.

1. **Khởi dựng bằng Docker Compose**:
   ```bash
   cd Lab06
   docker-compose up --build -d
   ```
2. **Kiểm tra trạng thái**:
   Ứng dụng chạy trên container sẽ lắng nghe tại cổng `5012` của máy thật: `http://localhost:5012`
   Dữ liệu cơ sở dữ liệu được lưu bền vững tại thư mục `./data/MedicalSuppliesLab06.db` ở máy thật.

---

## 👥 Tài khoản Demo (Đã seed sẵn)

Hệ thống phân quyền chi tiết thông qua các Role và Policy:

| Tài khoản | Mật khẩu | Vai trò (Role) | Mô tả quyền hạn |
|-----------|----------|----------------|-----------------|
| `admin@shop.test` | `Admin@123` | **Admin** | Toàn quyền (CRUD, Xóa mềm, Khôi phục, Tải ảnh, Đóng/mở cách ly, Xem Audit Logs, Điều chỉnh kho). |
| `staff@shop.test` | `Staff@123` | **Staff** | Đọc danh mục sản phẩm, lịch sử cấp phát, **chỉ có thêm quyền Điều chỉnh tồn kho** (Feature 1). Không được sửa giá, không được xóa, không xem được Audit Logs. |
| `user@shop.test` | `User@123` | **User** | Người dùng thông thường: Xem danh sách vật tư y tế và xem chi tiết. Bị chặn khỏi mọi trang quản trị khác. |

---

## 🔒 Các tính năng bảo mật nổi bật (Security Pack)

1. **Chống giả mạo yêu cầu (CSRF/XSRF)**:
   Mọi biểu mẫu gửi dữ liệu lên server (POST/PUT/DELETE) đều tích hợp Tag Helpers tự động tạo token ẩn, và được kiểm duyệt phía server bằng thuộc tính `[ValidateAntiForgeryToken]`.
2. **Chống tiêm mã độc (XSS)**:
   Razor View mặc định sử dụng HTML Encoding để hiển thị dữ liệu nhập từ người dùng. Tuyệt đối không dùng `@Html.Raw` với dữ liệu thô chưa qua bộ lọc.
3. **Chống tấn công SQL Injection**:
   Hệ thống sử dụng EF Core LINQ truy vấn, tự động sinh các câu lệnh SQL được tham số hóa (parameterized query), đảm bảo dữ liệu đầu vào không thể thay đổi cấu trúc câu lệnh SQL.
4. **Tải ảnh an toàn (Feature 2)**:
   - Whitelist định dạng: chỉ chấp nhận `.jpg`, `.jpeg`, `.png`, `.webp`.
   - Giới hạn dung lượng: tối đa `2MB`.
   - Tên tệp ngẫu nhiên dùng GUID để loại bỏ hoàn toàn lỗ hổng ghi đè tệp tin và tấn công Path Traversal.
   - Cơ chế thay ảnh an toàn: Chỉ xóa tệp ảnh cũ khi quá trình ghi tệp ảnh mới và cập nhật database thành công.
5. **Nhật ký kiểm toán bảo mật (Audit Logs - Feature 3)**:
   Mọi hành vi nhạy cảm (Đăng nhập thành công/thất bại, bị từ chối quyền, thêm/sửa/xóa vật tư, thay đổi tồn kho, thay đổi trạng thái cách ly) đều được ghi nhận chi tiết (Người thực hiện, Thời gian, Hành động, Đối tượng tác động, Kết quả, IP Client, Ghi chú) vào database. Chỉ có Admin mới được xem bảng nhật ký này tại `/AuditLogs`.

---

## 🌟 Chức năng sáng tạo thêm: Cách ly vật tư y tế (Quarantine)

Trong môi trường y tế, một số thiết bị hoặc vật tư có thể bị lỗi cảm biến, hết hạn kiểm chuẩn chất lượng hoặc có cảnh báo thu hồi từ nhà sản xuất.
- **Cách hoạt động**:
  - Admin có quyền đưa vật tư vào trạng thái **Cách ly (Quarantine)** kèm lý do.
  - Vật tư bị cách ly sẽ hiển thị cảnh báo đỏ nổi bật trên trang chi tiết và có nhãn "Cách ly" ở danh sách.
  - Hệ thống **khóa hoàn toàn chức năng tạo yêu cầu cấp phát kho (SupplyRequest)** đối với vật tư này. Mọi nỗ lực gửi POST request cố tình cấp phát vật tư bị cách ly đều bị chặn từ server-side bằng transaction nghiệp vụ và lưu log thất bại.

---

## 📈 Đo lường & Lỗi chuẩn hóa (Observability)

- **Health Checks**:
  - `/health/live`: Kiểm tra dịch vụ ứng dụng web có hoạt động hay không.
  - `/health/ready`: Kiểm tra khả năng kết nối cơ sở dữ liệu thật của ứng dụng.
- **ProblemDetails & ValidationProblemDetails (RFC 7807)**:
  - `GET /api/apisupplies/{id}`: Nếu ID không tồn tại, trả lỗi 404 ProblemDetails định dạng JSON có chứa `traceId` và `errorCode`.
  - `GET /api/apisupplies/search?keyword=`: Nếu từ khóa rỗng hoặc vượt quá 50 ký tự, trả lỗi 400 ValidationProblemDetails chi tiết các trường bị lỗi dữ liệu nhập.
