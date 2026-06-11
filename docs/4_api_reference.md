# 4. TÀI LIỆU API (API REFERENCE)

Tài liệu tham khảo chi tiết các endpoint backend (KIDIO.API).
Tất cả responses được bọc trong `ApiResponse<T>` trừ khi ghi chú khác.

## Base URLs

- Development HTTP: `http://localhost:5109`
- Development HTTPS: `https://localhost:7014`
- Staging/Production: Lấy từ biến môi trường của hệ thống.

---

## 1. Authentication (`/api/auth`)

### Xác thực Truyền thống (Local Account)

#### `POST /api/auth/register`
- Đăng ký tài khoản bằng Email/Password.
- Body: `{ Email, Password, ConfirmPassword, DisplayName }`
- Mật khẩu yêu cầu: ≥8 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.

#### `POST /api/auth/login`
- Đăng nhập bằng Email/Password.
- Body: `{ Email, Password }`
- Trả về: `AuthResponse { AccessToken, RefreshToken, ... }`

#### `GET /api/auth/verify-email?token={token}`
- Link xác thực được gửi qua email. Dùng để kích hoạt tài khoản.
- Trả về: HTML Page.

#### `POST /api/auth/resend-verification`
- Gửi lại email xác thực.
- Body: `{ Email }`

#### `POST /api/auth/forgot-password`
- Yêu cầu gửi email đặt lại mật khẩu.
- Body: `{ Email }`

#### `GET /api/auth/reset-password-page?token={token}`
- Link được nhúng trong email Forgot Password.
- Trả về: HTML Page (form nhập mật khẩu mới có validation real-time).

#### `POST /api/auth/reset-password`
- Đặt lại mật khẩu sử dụng Token gửi qua email (form HTML tự gọi endpoint này).
- Body: `{ Token, NewPassword, ConfirmNewPassword }`
- Mật khẩu mới phải đáp ứng đầy đủ ràng buộc độ mạnh.

#### `POST /api/auth/change-password`
- **Auth:** Bearer (Cần đăng nhập)
- Đổi mật khẩu cho người dùng hiện tại.
- Body: `{ OldPassword, NewPassword, ConfirmNewPassword }`
- Mật khẩu mới phải đáp ứng đầy đủ ràng buộc độ mạnh.

### Token Management & Google

#### `POST /api/auth/google`
- Đăng nhập bằng Google IdToken.
- Body: `{ IdToken }`

#### `POST /api/auth/refresh`
- Lấy Access Token mới thông qua Refresh Token.
- Body: `{ RefreshToken }`

#### `POST /api/auth/logout`
- **Auth:** Bearer
- Đăng xuất, vô hiệu hóa Refresh Token.

#### `GET /api/auth/me`
- **Auth:** Bearer
- Lấy thông tin User hiện tại từ JWT.

---

## 2. Child Management (`/api/child`)

> **Auth:** Bearer (tất cả endpoints)

#### `GET /api/child`
- Lấy danh sách hồ sơ trẻ của Phụ huynh hiện tại (phân trang).
- Query: `?pageNumber=1&pageSize=10`

#### `GET /api/child/{childId}`
- Lấy chi tiết một hồ sơ trẻ.

#### `POST /api/child`
- Tạo hồ sơ trẻ mới.
- Body: `CreateChildRequest { Name, DateOfBirth, AvatarUrl, ... }`

#### `PUT /api/child/{childId}`
- Cập nhật hồ sơ trẻ.
- Body: `UpdateChildRequest`

#### `DELETE /api/child/{childId}`
- Soft-Delete hồ sơ trẻ.

#### `PATCH /api/child/{childId}/restore`
- Khôi phục hồ sơ trẻ đã xóa mềm.

#### `DELETE /api/child/{childId}/hard`
- **Role:** Admin
- Hard-Delete hồ sơ trẻ vĩnh viễn.

---

## 3. Topic Management (`/api/topic`)

#### `GET /api/topic`
- Public. Lấy danh sách chủ đề (phân trang).
- Query: `?pageNumber=1&pageSize=10`

#### `GET /api/topic/{topicId}`
- Public. Lấy chi tiết một chủ đề.

#### `POST /api/topic`
- **Role:** Admin. Tạo chủ đề mới.
- Body: `CreateTopicRequest { Name, Description, OrderIndex, ThumbnailUrl, ... }`

#### `PUT /api/topic/{topicId}`
- **Role:** Admin. Cập nhật chủ đề.

#### `DELETE /api/topic/{topicId}`
- **Role:** Admin. Soft-Delete chủ đề.

#### `PATCH /api/topic/{topicId}/restore`
- **Role:** Admin. Khôi phục chủ đề đã xóa mềm.

#### `DELETE /api/topic/{topicId}/hard`
- **Role:** Admin. Hard-Delete chủ đề vĩnh viễn.

---

## 4. Lesson Management (`/api/lesson`)

#### `GET /api/lesson/all`
- Public (Admin thấy cả unpublished). Lấy toàn bộ bài học (phân trang).
- Query: `?pageNumber=1&pageSize=10`

#### `GET /api/lesson/topic/{topicId}`
- Public. Lấy danh sách bài học theo Chủ đề (phân trang).

#### `GET /api/lesson/{lessonId}`
- Public. Lấy chi tiết một bài học.

#### `POST /api/lesson`
- **Role:** Admin. Tạo bài học mới.
- Body: `CreateLessonRequest { TopicId, Title, Description, DifficultyLevel, SkillFocus, ... }`

#### `PUT /api/lesson/{lessonId}`
- **Role:** Admin. Cập nhật bài học.

#### `DELETE /api/lesson/{lessonId}`
- **Role:** Admin. Soft-Delete bài học.

#### `PATCH /api/lesson/{lessonId}/publish`
- **Role:** Admin. Publish (công khai) bài học.

#### `PATCH /api/lesson/{lessonId}/unpublish`
- **Role:** Admin. Unpublish (ẩn) bài học.

#### `PATCH /api/lesson/{lessonId}/restore`
- **Role:** Admin. Khôi phục bài học đã xóa mềm.

#### `DELETE /api/lesson/{lessonId}/hard`
- **Role:** Admin. Hard-Delete bài học vĩnh viễn.

---

## 5. Vocabulary Management (`/api/vocabulary`)

#### `GET /api/vocabulary/lesson/{lessonId}`
- Public. Lấy danh sách từ vựng theo Bài học (phân trang).

#### `GET /api/vocabulary/{vocabId}`
- Public. Lấy chi tiết một từ vựng.

#### `GET /api/vocabulary/paged`
- **Role:** Admin. Lấy tất cả từ vựng (phân trang, lọc theo lessonId tùy chọn).
- Query: `?page=1&pageSize=10&lessonId=<guid>`

#### `GET /api/vocabulary/all`
- **Role:** Admin. Lấy toàn bộ từ vựng (phân trang).

#### `GET /api/vocabulary/search`
- **Role:** Admin. Tìm kiếm từ vựng theo keyword và/hoặc lessonId.
- Query: `?keyword=apple&lessonId=<guid>&pageNumber=1&pageSize=10`

#### `POST /api/vocabulary`
- **Role:** Admin. Thêm từ vựng mới.
- Body: `CreateVocabularyRequest { LessonId, Word, Meaning, PhoneticText, ExampleSentence, ImageUrl, AudioUrl }`

#### `PUT /api/vocabulary/{vocabId}`
- **Role:** Admin. Cập nhật từ vựng.

#### `DELETE /api/vocabulary/{vocabId}`
- **Role:** Admin. Soft-Delete từ vựng.

#### `PATCH /api/vocabulary/{vocabId}/restore`
- **Role:** Admin. Khôi phục từ vựng đã xóa mềm.

#### `DELETE /api/vocabulary/{vocabId}/hard`
- **Role:** Admin. Hard-Delete từ vựng vĩnh viễn.

---

## 6. Progress (`/api/progress`)

> **Auth:** Bearer (tất cả endpoints)

#### `POST /api/progress/submit`
- Gửi kết quả hoàn thành bài học.
- Body: `SubmitProgressRequest { ChildId, LessonId, Score, TimeSpentSeconds, StarsEarned }`

#### `GET /api/progress/child/{childId}`
- Lấy toàn bộ lịch sử học của một Child (phân trang).

#### `GET /api/progress/child/{childId}/lesson/{lessonId}`
- Lấy tiến độ của một Child trên một Lesson cụ thể.

#### `GET /api/progress/child/{childId}/summary`
- Lấy dữ liệu tóm tắt học tập của Child (dùng cho Parent Dashboard).

#### `GET /api/progress/child/{childId}/recent-activities`
- Lấy các hoạt động học gần đây của Child (phân trang).

#### `GET /api/progress/child/{childId}/completed-lessons`
- Lấy danh sách bài học đã hoàn thành của Child (phân trang).

---

## 7. Pronunciation (`/api/pronunciation`)

> **Auth:** Bearer (tất cả endpoints)

#### `POST /api/pronunciation/submit`
- Upload file WAV và chấm điểm phát âm.
- Content-Type: `multipart/form-data`
- Form Fields: `VocabularyId` (Guid, bắt buộc), `LessonId` (Guid, tùy chọn), `AudioFile` (file WAV, bắt buộc)

#### `GET /api/pronunciation/vocabulary/{vocabularyId}`
- Lấy lịch sử phát âm của Child cho một từ vựng cụ thể.

#### `GET /api/pronunciation/history`
- Lấy toàn bộ lịch sử phát âm của Child hiện tại (phân trang).
- Query: `?pageNumber=1&pageSize=10`

---

## 8. Text-to-Speech (`/api/tts`)

#### `GET /api/tts/voices`
- Public. Lấy danh sách giọng đọc hỗ trợ.

#### `POST /api/tts/synthesize`
- **Auth:** Bearer. Tổng hợp giọng nói từ văn bản tùy chỉnh.
- Body: `TextToSpeechSynthesizeRequest { Text, VoiceName, Rate, Pitch }`

#### `POST /api/tts/lesson/{lessonId}`
- **Auth:** Bearer. Tổng hợp âm thanh cho toàn bộ nội dung của một Lesson.
- Body: `TextToSpeechOptionsRequest { VoiceName, Rate, Pitch }`

---

## 9. Achievement (`/api/achievement`)

> **Auth:** Bearer (tất cả endpoints)

#### `GET /api/achievement/child/{childId}`
- Lấy danh sách huy hiệu đã đạt được của một Child (phân trang).

### Quản lý Định nghĩa (Admin only)

#### `GET /api/achievement/definitions`
- **Role:** Admin. Lấy tất cả định nghĩa huy hiệu (phân trang, bao gồm inactive).

#### `GET /api/achievement/definitions/{id}`
- **Role:** Admin. Lấy chi tiết một định nghĩa huy hiệu.

#### `POST /api/achievement/definitions`
- **Role:** Admin. Tạo mới loại Danh hiệu.
- Body: `CreateAchievementDefinitionRequest { Name, Description, BadgeImageUrl, Threshold, ... }`

#### `PUT /api/achievement/definitions/{id}`
- **Role:** Admin. Cập nhật định nghĩa Danh hiệu.

#### `DELETE /api/achievement/definitions/{id}`
- **Role:** Admin. Soft-Delete định nghĩa Danh hiệu.

#### `PATCH /api/achievement/definitions/{id}/restore`
- **Role:** Admin. Khôi phục định nghĩa Danh hiệu đã xóa mềm.

#### `DELETE /api/achievement/definitions/{id}/hard`
- **Role:** Admin. Hard-Delete định nghĩa Danh hiệu vĩnh viễn.

---

## 10. Parent Dashboard (`/api/parentdashboard`)

> **Auth:** Bearer

#### `GET /api/parentdashboard/overview`
- Lấy tổng quan học tập của tất cả con thuộc Phụ huynh hiện tại.
- Query: `?weeks=4` (tùy chọn số tuần thống kê, mặc định 4 tuần)

---

> *Chi tiết Schema và cấu trúc DTO có thể tham khảo trực tiếp trong thư mục `KIDIO.Business/DTOs` và **Swagger** (tại URL `/swagger` trên môi trường Dev).*
