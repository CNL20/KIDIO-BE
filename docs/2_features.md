# 2. Danh sách Chức năng (Features)

Hệ thống KIDIO được chia thành nhiều Module chức năng khác nhau để quản lý nội dung học tập và người dùng.

## 1. Authentication (Xác thực & Bảo mật)
- **Đăng nhập / Đăng ký truyền thống**: Người dùng có thể tạo tài khoản qua Email/Mật khẩu. Mật khẩu yêu cầu tối thiểu 8 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.
- **Xác thực Email**: Gửi link xác thực qua email để kích hoạt tài khoản. Hỗ trợ gửi lại email xác thực.
- **Đăng nhập Google**: Hỗ trợ Google Sign-In thông qua ID Token.
- **Quản lý Mật khẩu**:
  - **Đổi mật khẩu** (Change Password): User đã đăng nhập có thể đổi mật khẩu (yêu cầu mật khẩu cũ). Mật khẩu mới cũng phải đáp ứng đầy đủ ràng buộc độ mạnh.
  - **Quên mật khẩu** (Forgot Password): Gửi email kèm link Reset Password.
  - **Đặt lại mật khẩu** (Reset Password): Người dùng click link trong email → được chuyển đến trang HTML nhập mật khẩu mới → submit API để đặt lại. Mật khẩu mới cũng phải đáp ứng đầy đủ ràng buộc độ mạnh.
- **JWT & Refresh Token**: Cấp phát và thu hồi Token để duy trì phiên đăng nhập an toàn.

## 2. Quản lý Hồ sơ Người dùng (Child Management)
- Phụ huynh có thể tạo, cập nhật, lấy danh sách, và lấy chi tiết các hồ sơ trẻ em (Child Profile).
- Hỗ trợ Soft-Delete (xóa mềm), Khôi phục (Restore), và Hard-Delete (dành cho Admin).
- Mỗi trẻ em được lưu vết `TotalStars` (tổng số sao), `CurrentStreakDays` (chuỗi ngày học liên tục).

## 3. Quản lý Chủ đề (Topics) & Bài học (Lessons)
- **Topics**: Chứa nhiều bài học. Admin có thể tạo, cập nhật, sắp xếp vị trí (OrderIndex). Hỗ trợ Soft-Delete, Restore, Hard-Delete.
- **Lessons**: Phân loại theo độ khó, kỹ năng (SkillFocus). Hỗ trợ nội dung đa phương tiện (Thumbnail, Audio, Video, JSON Content). Admin có thể `Publish` (công khai) hoặc `Unpublish` (ẩn) bài học. Hỗ trợ Soft-Delete, Restore, Hard-Delete.

## 4. Quản lý Từ vựng (Vocabularies)
- Từ vựng được gắn với từng Bài học.
- Hỗ trợ thông tin: Từ (Word), Nghĩa (Meaning), Phiên âm (PhoneticText), Hình ảnh minh họa, Câu ví dụ, File âm thanh mẫu.
- Cho phép tìm kiếm (Search) từ vựng theo keyword và lọc theo Lesson.
- Hỗ trợ Soft-Delete, Restore, Hard-Delete (Admin only).

## 5. Tiến độ Học tập (Progress)
- Khi hoàn thành bài học, Frontend gửi kết quả (Score, TimeSpent, StarsEarned) lên API.
- Cập nhật số sao (Stars) và chuỗi ngày học (Streak) cho trẻ.
- Tính toán và tự động cấp Danh hiệu (Achievements) nếu đủ điều kiện.
- Cung cấp:
  - Dữ liệu tóm tắt (Summary) theo từng Child.
  - Lịch sử hoạt động gần nhất (Recent Activities).
  - Danh sách bài học đã hoàn thành (Completed Lessons).
  - Tiến độ cụ thể theo từng Child + Lesson.

## 6. Luyện Phát Âm (Pronunciation)
- Cho phép tải lên file ghi âm (WAV) của trẻ cho một từ vựng cụ thể.
- Xử lý file, kết nối tới dịch vụ chấm điểm phát âm (Accuracy, Fluency, Completeness).
- Lưu trữ lịch sử các lần đọc, lưu điểm số cao nhất (Best Score) và phản hồi (Feedback) chi tiết.
- Cung cấp lịch sử phát âm theo từng từ vựng và danh sách toàn bộ lịch sử theo Child (phân trang).

## 7. Text-to-Speech (TTS)
- Cung cấp tính năng tổng hợp giọng nói từ văn bản dành cho bài học hoặc từ vựng.
- Hỗ trợ chọn giọng đọc (Voice), Tốc độ (Rate), và Cao độ (Pitch).
- Sinh file âm thanh tự động và lưu trữ bộ nhớ đệm (Cache) để tái sử dụng.
- Hỗ trợ tổng hợp âm thanh theo từng Lesson (`POST /api/tts/lesson/{id}`).
- Cung cấp danh sách giọng đọc hỗ trợ (`GET /api/tts/voices`).

## 8. Danh hiệu & Thành tích (Achievements)
- Admin quản lý các định nghĩa Danh hiệu (Achievement Definitions) với các ngưỡng (Threshold) khác nhau. Hỗ trợ CRUD đầy đủ bao gồm Soft-Delete, Restore, Hard-Delete.
- Trẻ em có thể xem danh sách các huy hiệu (Badges) đã thu thập được trong quá trình học.

## 9. Dashboard Phụ huynh (Parent Dashboard)
- Cung cấp cái nhìn tổng quan (`/api/parentdashboard/overview`) về tình hình học tập của tất cả các con.
- Hỗ trợ tuỳ chọn số tuần thống kê (mặc định 4 tuần).
- Đưa ra thống kê và biểu đồ hoạt động cơ bản.
