# Hướng Dẫn Cấu Hình & Chạy Demo Dự Án KIDIO

Tài liệu này hướng dẫn chi tiết các bước cần thực hiện ngay sau khi kéo (pull) mã nguồn mới nhất về máy để chạy thử nghiệm (demo) cả Backend và Frontend, bao gồm cả việc xác định và cấu hình địa chỉ IP cục bộ để thiết bị di động (thật) kết nối được với máy chủ API.

---

## BƯỚC 1: Xác Định Địa Chỉ IP Cục Bộ (Local IP) của Máy Tính
Nếu bạn chạy ứng dụng Flutter trên **thiết bị di động thật** (kết nối cùng Wi-Fi với máy tính), thiết bị di động không thể dùng `localhost` để kết nối tới backend. Bạn cần tìm IP của máy tính trong mạng nội bộ:

1. Mở **PowerShell** hoặc **Command Prompt** trên Windows.
2. Gõ lệnh:
   ```cmd
   ipconfig
   ```
3. Tìm đến adapter mạng đang hoạt động (ví dụ: *Wireless LAN adapter Wi-Fi* hoặc *Ethernet adapter*).
4. Lưu lại địa chỉ **IPv4 Address** (ví dụ: `192.168.1.10` hoặc `10.39.244.5`).

*Lưu ý: Đảm bảo điện thoại di động và máy tính chạy backend của bạn đang kết nối vào **cùng một mạng Wi-Fi**.*

---

## BƯỚC 2: Cấu Hình & Chạy Backend (C# Web API & PostgreSQL)

### 1. Khởi chạy Database (PostgreSQL)
Bạn có thể sử dụng Docker để khởi chạy nhanh cơ sở dữ liệu mà không cần cài đặt PostgreSQL lên hệ điều hành:

1. Mở **Docker Desktop** trên máy tính.
2. Di chuyển vào thư mục backend:
   ```bash
   cd backend/KIDIO-BE
   ```
3. Khởi chạy container PostgreSQL (được cấu hình sẵn cổng `5432` và mật khẩu `12345` trong tệp `docker-compose.yml`):
   ```bash
   docker compose up -d db
   ```

### 2. Cấu hình biến môi trường (.env)
1. Kiểm tra sự tồn tại của tệp `.env` tại:
   * `backend/KIDIO-BE/.env`
   * `backend/KIDIO-BE/KIDIO.API/.env`
2. Nếu chưa có, sao chép từ tệp `.env.example` tương ứng.
3. Cập nhật các biến sau bằng **Địa chỉ IP cục bộ** bạn vừa tìm được ở Bước 1:
   ```properties
   # URL Backend để CORS và OAuth nhận diện chính xác
   UrlSettings__BackendUrl="http://<IP_CUC_BO_CUA_BAN>:5109"
   
   # Cho phép frontend từ thiết bị di động kết nối (CORS)
   Cors__AllowedOrigins__3="http://<IP_CUC_BO_CUA_BAN>:3000"
   ```
   *Ví dụ thực tế:*
   `UrlSettings__BackendUrl="http://192.168.1.10:5109"`

### 3. Chạy Backend API
1. Di chuyển vào thư mục dự án API:
   ```bash
   cd KIDIO.API
   ```
2. Chạy lệnh khởi động ứng dụng:
   ```bash
   dotnet run
   ```
   * **Tự động di chuyển dữ liệu (Migrations & Seeding)**: Backend được thiết lập cơ chế tự động chạy Migration để dựng bảng database và nạp dữ liệu mẫu (chủ đề, bài học, từ vựng, huy hiệu) ngay khi khởi động. Bạn không cần chạy lệnh update database thủ công.
   * **Swagger API**: Sau khi chạy thành công, bạn có thể truy cập `http://localhost:5109/swagger` từ trình duyệt của máy tính để test các API endpoint.

---

## BƯỚC 3: Cấu Hình & Chạy Frontend (Flutter)

### 1. Cài đặt thư viện dependencies
1. Di chuyển vào thư mục frontend:
   ```bash
   cd frontend/KIDIO-Frontend
   ```
2. Cài đặt các package của Flutter:
   ```bash
   flutter pub get
   ```

### 2. Cấu hình địa chỉ IP máy chủ API
Để ứng dụng Flutter gọi đúng địa chỉ máy chủ API cục bộ của bạn, bạn có 2 cách cấu hình:

#### Cách A: Sửa trực tiếp trong code (Được khuyến nghị cho nhanh)
1. Mở tệp `lib/api/api_client.dart`.
2. Tìm đến hàm getter `_baseUrl` (khoảng dòng 20-28):
   ```dart
   static String get _baseUrl {
     const envUrl = String.fromEnvironment('API_URL');
     if (envUrl.isNotEmpty) {
       return envUrl;
     }
     // Sửa địa chỉ IP tại đây thành IP cục bộ của máy tính bạn
     return 'http://<IP_CUC_BO_CUA_BAN>:5109/api/';
   }
   ```
   *Ví dụ:* `return 'http://192.168.1.10:5109/api/';`

#### Cách B: Truyền qua tham số dòng lệnh khi chạy ứng dụng
Nếu không muốn sửa code, bạn có thể truyền trực tiếp URL API qua tham số `--dart-define` khi khởi chạy:
```bash
flutter run --dart-define=API_URL=http://<IP_CUC_BO_CUA_BAN>:5109/api/
```

### 3. Chạy ứng dụng Flutter
1. Kết nối điện thoại thật của bạn vào máy tính qua cáp USB (đã bật *Chế độ nhà phát triển* và *Gỡ lỗi USB - USB Debugging*), hoặc mở Trình giả lập (Emulator).
2. Kiểm tra danh sách thiết bị nhận diện được:
   ```bash
   flutter devices
   ```
3. Chạy ứng dụng lên thiết bị mong muốn:
   ```bash
   flutter run -d <ID_THIET_BI>
   ```

---

## Mẹo khắc phục lỗi kết nối (Troubleshooting)
* **Lỗi `Connection refused` hoặc `Connection timeout`**:
  * Kiểm tra lại xem điện thoại và máy tính đã kết nối chung 1 Wi-Fi chưa.
  * Hãy tắt tạm thời **Windows Defender Firewall** (hoặc tạo Rule cho phép Port `5109` nhận kết nối inbound) trên máy tính chạy backend, vì tường lửa Windows thường chặn các kết nối lạ từ thiết bị di động.
  * Kiểm tra xem backend API đã chạy trên máy tính chưa (thử truy cập link Swagger trên trình duyệt máy tính).
