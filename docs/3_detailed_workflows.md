# 3. Luồng Hoạt Động (Detailed Workflows)

Tài liệu này giải thích chi tiết cách các Frontend (Client) nên tương tác với hệ thống Backend theo từng luồng nghiệp vụ cụ thể.

## 1. Luồng Xác thực (Authentication Flow)

**Trường hợp 1: Sử dụng Local Account (Email / Password)**
1. Client gọi `POST /api/auth/register` với thông tin Email, Password, ConfirmPassword, DisplayName.
   - Mật khẩu phải: ≥ 8 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.
2. Hệ thống lưu User (IsEmailConfirmed = false) và gửi một Email chứa Link xác thực kèm Token.
3. Người dùng check Email, click vào link `GET /api/auth/verify-email?token=...` → nhận trang HTML xác nhận kết quả.
4. Sau khi xác thực, Client gọi `POST /api/auth/login` với Email và Password.
5. Server trả về `AccessToken` (hết hạn nhanh) và `RefreshToken` (thời hạn lâu). Client dùng AccessToken đưa vào Header `Authorization: Bearer <Token>` để gọi các API khác.

**Trường hợp 2: Sử dụng Google Sign-In**
1. Client sử dụng Google SDK để lấy được `IdToken` từ Google.
2. Client gọi `POST /api/auth/google` và gửi `IdToken`.
3. Server xác thực Token với Google. Nếu tài khoản chưa tồn tại, tự động tạo mới (cập nhật Avatar).
4. Server trả về `AccessToken` và `RefreshToken`.

**Duy trì phiên đăng nhập:**
- Khi `AccessToken` hết hạn (nhận mã lỗi 401), Client gọi `POST /api/auth/refresh` kèm theo `RefreshToken` hiện tại để lấy cặp Token mới.

## 2. Luồng Đặt lại Mật khẩu (Forgot / Reset Password Flow)

1. Client gọi `POST /api/auth/forgot-password` với `{ Email }`.
2. Server tạo một `ResetToken` ngẫu nhiên, lưu vào DB kèm thời hạn 1 giờ, gửi Email chứa link dạng:
   `GET /api/auth/reset-password-page?token=<ResetToken>`.
3. Người dùng click link trong Email → Trình duyệt mở trang HTML (ResetPasswordPage) với form nhập mật khẩu mới.
4. Người dùng điền mật khẩu mới (cũng phải đáp ứng ràng buộc độ mạnh), form tự động gọi:
   `POST /api/auth/reset-password` với `{ Token, NewPassword, ConfirmNewPassword }`.
5. Server xác thực Token, cập nhật mật khẩu mới, xóa Token → người dùng đăng nhập lại bình thường.

## 3. Luồng Học Bài (Lesson Flow) & Gửi Tiến độ (Progress)

1. **Chọn bài học:** Người dùng (Child) xem danh sách Chủ đề (`GET /api/topic`) và Bài học (`GET /api/lesson/topic/{id}`).
2. **Học từ vựng:** Lấy danh sách từ vựng của bài học (`GET /api/vocabulary/lesson/{id}`). Client có thể gọi API Text-to-Speech nếu cần phát âm mẫu.
3. **Luyện phát âm:** Client ghi âm giọng đọc của Child (định dạng WAV) và upload lên `POST /api/pronunciation/submit`. Server trả về điểm số chi tiết từng phần.
4. **Hoàn thành bài học:** Sau khi hoàn tất bài kiểm tra hoặc tương tác, Client gửi tổng kết lên `POST /api/progress/submit` (gồm Điểm số, Thời gian hoàn thành).
5. **Nhận kết quả:** Server tính toán số Sao nhận được, cập nhật Chuỗi ngày học (Streak), kiểm tra các Danh hiệu mới (Achievements) và trả về cho Client để hiển thị hiệu ứng chúc mừng (Wow effect).

## 4. Luồng Upload File Ghi Âm (Multipart Upload Flow)

1. Yêu cầu định dạng File âm thanh là `.wav`.
2. Client tạo request sử dụng `multipart/form-data`.
3. Các field cần đính kèm trong Form Data:
   - `VocabularyId` (Bắt buộc): GUID của từ vựng đang đọc.
   - `LessonId` (Tùy chọn): GUID của bài học.
   - `AudioFile` (Bắt buộc): File âm thanh vật lý thực sự.
4. Gửi POST request tới `/api/pronunciation/submit`.

## 5. Luồng Quản trị Dữ liệu (Admin CRUD Flow)

- Chỉ có tài khoản mang Role `Admin` mới có quyền thực hiện (Tài khoản được gán quyền thông qua cấu hình `AdminSettings:Emails` trong appsettings).
- Khi Admin tạo (Ví dụ: `POST /api/topic`), dữ liệu sẽ được lưu vào hệ thống.
- Khi Admin xóa (`DELETE /api/topic/{id}`), dữ liệu chỉ được gán cờ `IsDeleted = true` (Soft Delete) chứ không mất hẳn. Các API truy vấn thông thường sẽ tự động lọc các dữ liệu này ra.
- Nếu Admin lỡ xóa, có thể gọi `PATCH /api/topic/{id}/restore`.
- Nếu Admin muốn dọn dẹp bộ nhớ, gọi `DELETE /api/topic/{id}/hard` để xóa vĩnh viễn khỏi Database.
- Tương tự áp dụng cho: **Lesson**, **Vocabulary**, **AchievementDefinitions**.
