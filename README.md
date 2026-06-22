# KIDIO (Backend API)

Hệ thống học ngôn ngữ dành cho trẻ em (KIDIO) hỗ trợ các chức năng quản lý bài học, từ vựng, luyện phát âm, chuyển đổi văn bản thành giọng nói (TTS) và bảng điều khiển cho phụ huynh.

Tài liệu chi tiết về dự án đã được chia nhỏ và tổ chức gọn gàng trong thư mục `docs/`. Vui lòng tham khảo các tệp sau để biết thêm chi tiết:

- [1. Giới thiệu Dự án & Công nghệ (Introduction & Technologies)](docs/1_introduction_and_technologies.md)
- [2. Danh sách Chức năng (Features)](docs/2_features.md)
- [3. Luồng Hoạt động (Detailed Workflows)](docs/3_detailed_workflows.md)
- [4. Tài liệu API (API Reference)](docs/4_api_reference.md)

---

## Bắt đầu nhanh (Quick Start)

### Yêu cầu hệ thống
- .NET 7.0 SDK
- PostgreSQL Database

### Môi trường (Environment Variables)
Tạo tệp `appsettings.Development.json` dựa trên mẫu của bạn và thiết lập các biến sau:
- `ConnectionStrings:DefaultConnection`
- `JwtSettings:SecretKey`, `Issuer`, `Audience`
- `GoogleOAuth:ClientId`, `GoogleOAuth:ClientSecret`
- `AzureSpeech:AzureSpeechKey`, `AzureSpeech:AzureSpeechRegion`
- `AdminSettings:Emails` (Danh sách email được cấp quyền Admin)

### Chạy ứng dụng
```bash
cd KIDIO.API
dotnet run
```
Truy cập `https://localhost:7014/swagger` để xem giao diện thử nghiệm API.
