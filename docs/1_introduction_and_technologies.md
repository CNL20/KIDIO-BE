# 1. Giới thiệu Dự án & Công nghệ

## Tổng quan Hệ thống (KIDIO)
KIDIO là một hệ thống (Backend API) được thiết kế đặc biệt nhằm hỗ trợ quá trình học ngôn ngữ (tiếng Anh) cho trẻ em. Hệ thống cung cấp các bài học, từ vựng, luyện phát âm, chuyển đổi văn bản thành giọng nói (Text-to-Speech) và bảng điều khiển theo dõi tiến độ dành cho phụ huynh.

Hệ thống cung cấp cơ sở dữ liệu để lưu trữ tiến trình học, điểm số, cấp phát huy hiệu (achievements) và cung cấp các công cụ quản trị (Admin) để quản lý nội dung học tập.

## Đối tượng sử dụng
- **Phụ huynh (Parents)**: Đăng ký tài khoản, quản lý hồ sơ của các con, theo dõi tiến độ, xem báo cáo học tập.
- **Trẻ em (Children)**: Tương tác qua hồ sơ được phụ huynh tạo, học bài học, luyện phát âm, nhận sao thưởng và danh hiệu.
- **Quản trị viên (Admin)**: Quản lý Topics, Lessons, Vocabularies, Achievement definitions.
- **Client (Frontend)**: Mobile App / Web App kết nối thông qua các RESTful API.

## Công nghệ sử dụng
- **Ngôn ngữ & Framework**: C# / ASP.NET Core 8.0 (`.NET 8`).
- **Cơ sở dữ liệu**: PostgreSQL thông qua Entity Framework Core (EF Core).
- **Kiến trúc**: Layered Architecture (API -> Business -> Data).
- **Xác thực (Authentication)**:
  - JWT (JSON Web Token) với cấu hình Access Token & Refresh Token.
  - OAuth2.0: Google Sign-In.
- **Validation**: FluentValidation (tự động quét validators từ assembly).
- **Tích hợp bên ngoài (Third-party Services)**:
  - Text-to-Speech: Azure Speech Services.
  - Email: SMTP (thông qua dịch vụ EmailService nội bộ, cấu hình qua `appsettings`).
  - Phân tích phát âm (Pronunciation Scoring): Azure Speech Services / Custom AI Services.
- **Tài liệu API**: Swagger / OpenAPI (chỉ bật ở môi trường Development).
- **Lưu trữ tệp (File Storage)**: Local Storage tĩnh cho file âm thanh TTS (WAV) và ghi âm phát âm. *(Khuyến nghị dùng Azure Blob Storage hoặc AWS S3 cho môi trường Production.)*
