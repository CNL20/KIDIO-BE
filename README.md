# KIDIO API

Tài liệu tóm tắt và hướng dẫn nhanh cho project KIDIO (backend - ASP.NET Core).

---

## 1. Tổng quan hệ thống

### Mục tiêu project

- Hệ thống học ngôn ngữ cho trẻ em (KIDIO) hỗ trợ bài học, từ vựng, luyện phát âm, text-to-speech (TTS) và dashboard cho phụ huynh.
- Lưu trữ tiến độ, cấp huy hiệu/achievement, và hỗ trợ quản trị nội dung bởi Admin.

### Đối tượng sử dụng

- Phụ huynh (Parents): quản lý profile con, xem tiến độ, báo cáo.
- Trẻ em (Children): học lesson, luyện phát âm, xem kết quả.
- Quản trị viên (Admin): quản lý Topics, Lessons, Vocabularies, Achievements.
- Frontend: mobile/web app gọi API.

### Công nghệ sử dụng

- Backend: ASP.NET Core (C#), Entity Framework Core
- DB: Microsoft SQL Server (via EF Core `KidioDbContext`)
- Authentication: JWT (access + refresh), Google Sign-In for login
- API docs: Swagger/OpenAPI (dev)
- File storage: local static folders for TTS and pronunciations (production: recommend blob storage)

### Kiến trúc hệ thống

- Layered: Controllers (API) → Business Services (`KIDIO.Business`) → Data (Entities/Repositories/UnitOfWork)
- Cross-cutting: `ExceptionMiddleware`, JWT authentication, static file serving for audio

---

## 2. Phân tích chức năng

### Danh sách chức năng chính

- Authentication: Google login, refresh, logout, me
- Child management: CRUD, restore, hard-delete
- Topics & Lessons: list, detail, Admin CRUD, publish/unpublish
- Vocabularies: CRUD, search, list-by-lesson
- Progress: submit progress, get history, summary, recent activities
- Pronunciation: upload WAV for scoring, history
- Text-to-Speech: synthesize text/lesson, list voices
- Achievements: view earned, Admin manage definitions
- Parent dashboard: overview per child

### Mô tả ngắn từng chức năng

- Xem controllers trong `KIDIO.API/Controllers` để tham khảo chi tiết: `AuthController`, `ChildController`, `LessonController`, `TopicController`, `VocabularyController`, `ProgressController`, `PronunciationController`, `TextToSpeechController`, `AchievementController`, `ParentDashboardController`.

### Roles và quyền

- Anonymous (AllowAnonymous): đọc public resources (lessons, topics, vocab by lesson, TTS voices)
- Authenticated user: child/parent actions, submit progress, pronunciation uploads, parent dashboard
- Admin (Role = "Admin"): tạo/ cập nhật/ xóa/ restore nội dung (Topics, Lessons, Vocabularies, Achievement definitions), xem/paged admin endpoints

---

## 3. Backend API cho Frontend (tóm tắt)

Tất cả endpoint trả wrapper `ApiResponse<T>`.

Một số endpoint tiêu biểu:

- `POST /api/auth/google` — Google login
  - Body: `{ IdToken: string }`
  - Response: `AuthResponse` → `{ AccessToken, RefreshToken, AccessTokenExpiry, User }`
  - Auth: AllowAnonymous

- `POST /api/auth/refresh` — refresh access token
  - Body: `{ RefreshToken: string }`
  - Response: `AuthResponse`

- `GET /api/lesson/all` — get paged lessons
  - Query: `pageNumber`, `pageSize`
  - Auth: AllowAnonymous (Admin sees unpublished)

- `POST /api/pronunciation/submit` — upload WAV for scoring
  - Content-Type: `multipart/form-data`
  - Form fields: `VocabularyId` (guid), optional `LessonId`, `AudioFile` (WAV, max ~10MB)
  - Response: `PronunciationScoreResponse` (accuracy, fluency, completeness, overall, feedback, AudioStorageUrl)
  - Auth: Bearer

- `POST /api/progress/submit` — submit lesson result
  - Body: `{ ChildId, LessonId, ScorePercent, TimeSpentSeconds }`
  - Response: `ProgressResponse`
  - Auth: Bearer

- Admin CRUD examples: `POST /api/lesson`, `PUT /api/lesson/{id}`, `DELETE /api/lesson/{id}`, `PATCH /api/lesson/{id}/publish` (Admin only)

Chi tiết toàn bộ endpoint và DTOs nằm trong mã nguồn: `KIDIO.Business/DTOs` và `KIDIO.API/Controllers`.

---

## 4. Database (tổng quan entities)

Các bảng chính (suy ra từ DTOs và BaseEntity):

- `Users` (accounts) — Id, Email, DisplayName, AvatarUrl, Role, CreatedAt, UpdatedAt, IsDeleted
- `RefreshTokens` — UserId (FK), Token, ExpiresAt, RevokedAt
- `Children` — ParentUserId (FK), Name, Age, AvatarUrl, TotalStars, CurrentStreakDays, LastLessonAt
- `Topics` — Name, Description, OrderIndex, IsDeleted
- `Lessons` — Title, LessonType, Difficulty, SkillFocus, DurationSeconds, ThumbnailUrl, AudioUrl, VideoUrl, ContentJson, TopicId, IsPublished
- `Vocabularies` — Word, Meaning, PhoneticText, AudioUrl, ImageUrl, ExampleSentence, OrderIndex, LessonId
- `Progress` — ChildId, LessonId, StarsEarned, ScorePercent, TimeSpentSeconds, AttemptCount, CompletedAt
- `PronunciationLogs` — VocabularyId, (optional) LessonId, AudioStorageUrl, AccuracyScore, FluencyScore, CompletenessScore, OverallScore, Feedback
- `AchievementDefinitions`, `AchievementEarned`

Quan hệ: User → Children (1:N); Topic → Lessons (1:N); Lesson → Vocabularies (1:N); Child → Progress/Pronunciation/Achievements (1:N).

---

## 5. Frontend cần được cung cấp gì

- Gọi API: danh sách endpoint ở phần 3 (Auth, Lesson, Topic, Vocabulary, Progress, Pronunciation, TTS, Achievement, ParentDashboard).
- Dữ liệu trả về: `ApiResponse<T>`; pages: `PagedResponse<T>` hoặc DTO-specific paged wrappers.
- Tokens: lưu `AccessToken` (ngắn hạn), `RefreshToken` (dài hạn) — khuyến nghị lưu refresh token an toàn (httpOnly cookie). Gửi access token trong header `Authorization: Bearer <token>`.
- Upload file: Pronunciation upload via `multipart/form-data` to `/api/pronunciation/submit`.
- Realtime/notification: không có SignalR/Realtime tích hợp hiện tại; cần thêm nếu cần push notifications.

---

## 6. Luồng hoạt động hệ thống (tóm tắt)

### Flow đăng nhập

1. Client lấy Google IdToken.
2. `POST /api/auth/google` (body `{ IdToken }`) → server trả `AuthResponse`.
3. Client dùng `AccessToken` trong header; khi hết hạn gọi `POST /api/auth/refresh`.
4. `POST /api/auth/logout` để revoke refresh token.

### Flow CRUD

- Create: Admin/parent gửi POST DTO → server validate → trả `201 Created`.
- Read: GET (public hoặc protected)
- Update: PUT
- Delete: soft-delete (IsDeleted) bằng `DELETE`; hard-delete có endpoint `/hard` (Admin).

### Flow upload file (pronunciation)

- Client gửi `multipart/form-data` với WAV file và `VocabularyId` → server lưu file, chạy scoring, lưu log, trả `PronunciationScoreResponse`.

---

## 7. Deployment

### Docker (gợi ý nhanh)

- Tạo `Dockerfile` multistage: build (sdk) → publish → runtime `mcr.microsoft.com/dotnet/aspnet:7.0` (hoặc version tương ứng).
- Expose port và set env vars (ConnectionStrings, JwtSettings, GoogleOAuth, AzureSpeech etc.).

Ví dụ chạy local (Development):

```bash
cd KIDIO.API
dotnet run
```

Docker (ví dụ):

```bash
docker build -t kidio-api .
docker run -e ConnectionStrings__DefaultConnection="Server=...;Database=Kidio;User Id=...;Password=...;" -e JwtSettings__SecretKey="..." -p 5000:80 kidio-api
```

### Environment variables / appsettings (chủ yếu)

- `ConnectionStrings:DefaultConnection`
- `JwtSettings:SecretKey`, `Issuer`, `Audience`, `AccessTokenExpiryMinutes`, `RefreshTokenExpiryDays`
- `GoogleOAuth:ClientId`, `GoogleOAuth:ClientSecret`
- `AISettings:OpenAIApiKey`, `AzureSpeech:AzureSpeechKey`, `AzureSpeech:AzureSpeechRegion`
- `AdminSettings:Emails` (danh sách email admin để seed)

### Swagger/OpenAPI

- Swagger được cấu hình trong `Program.cs`. Hiển thị và sử dụng trong `Development` environment tại `/swagger`.

### Cách chạy project (local)

1. Cấu hình `appsettings.Development.json` với `ConnectionStrings:DefaultConnection`.
2. Chạy migrations hoặc để `Program.cs` tự migrate trong Development.
3. Từ folder `KIDIO.API`: `dotnet run`.
4. Dùng Swagger để thử các endpoint.

---

## Tệp quan trọng

- `KIDIO.API/Program.cs` — cấu hình app
- `KIDIO.API/Controllers/*` — API endpoints
- `KIDIO.Business/DTOs/*` — request/response shapes
- `KIDIO.Common/*` — shared settings, base entity
- `KIDIO.API/Middleware/ExceptionMiddleware.cs` — error handling

---

## Gợi ý cải tiến

- Lưu refresh token an toàn (httpOnly cookie) thay vì storage client-side.
- Dùng cloud blob storage cho audio/TTS trong production.
- Thêm SignalR nếu cần realtime/notifications.
- Thêm rate-limiting và upload size limits.

---

Nếu bạn muốn tôi:

- tạo `Dockerfile` mẫu và `docker-compose` (API + SQL Server) — tôi có thể tạo ngay;
- xuất file ER diagram (PlantUML) hoặc file markdown chi tiết endpoint từng route — tôi sẽ sinh tiếp.
