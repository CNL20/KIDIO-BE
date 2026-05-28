# KIDIO API Reference

Tài liệu tham khảo chi tiết các endpoint backend (KIDIO.API).
Tất cả responses được bọc trong `ApiResponse<T>` trừ khi ghi chú khác.

## Base URLs

- Development HTTP: `http://localhost:5109`
- Development HTTPS: `https://localhost:7014`
- Staging/Production: chưa thấy cấu hình cố định trong source hiện tại; FE cần lấy từ biến môi trường hoặc config deploy.

---

## Common types

- ApiResponse<T>: wrapper chung (ok/fail/message/data)
- PagedResponse<T>: common paged result (Items, Page, PageSize, TotalCount, TotalPages)

---

## 1. Authentication (`AuthController`)

### POST /api/auth/google

- Auth: AllowAnonymous
- Description: Login with Google IdToken
- Request body: `GoogleLoginRequest` { IdToken: string }
- Response: `ApiResponse<AuthResponse>`
  - `AuthResponse` { AccessToken:string, RefreshToken:string, AccessTokenExpiry:DateTime, User:{ Id:Guid, Email:string, DisplayName:string, AvatarUrl?:string, Role:string } }
- Status codes: 200 OK, 400/401 on invalid token

### POST /api/auth/refresh

- Auth: AllowAnonymous
- Request body: `RefreshTokenRequest` { RefreshToken: string }
- Response: `ApiResponse<AuthResponse>` (new tokens)
- Status: 200, 401 if invalid

### POST /api/auth/logout

- Auth: Bearer
- Request: none
- Response: `ApiResponse<object>` confirmation
- Status: 200

### GET /api/auth/me

- Auth: Bearer
- Request: none
- Response: `ApiResponse<object>` with claims { Id, Email, Name, Role }
- Status: 200

---

## 2. Child management (`ChildController`)

All endpoints require Bearer auth.

### GET /api/child

- Query: `pageNumber` (default 1), `pageSize` (default 10)
- Response: `ApiResponse<PagedResponse<ChildSummaryResponse>>`
- `ChildSummaryResponse`:
  - `Id: Guid`
  - `Name: string`
  - `Age: int`
  - `AvatarUrl?: string`
  - `TotalStars: int`
  - `CurrentStreakDays: int`
- Business note: nếu `Items: []` thì parent hiện chưa có profile con nào; FE nên hiển thị empty state và có thể điều hướng sang màn hình tạo profile child.
- Status: 200

### GET /api/child/{childId}

- Response: `ApiResponse<ChildResponse>`
- `ChildResponse`:
  - `Id: Guid`
  - `Name: string`
  - `Age: int`
  - `AvatarUrl?: string`
  - `TotalStars: int`
  - `CurrentStreakDays: int`
  - `LastLessonAt?: DateTime`
  - `CreatedAt: DateTime`
  - `IsRecommendedAge: bool`
- Business note: `IsRecommendedAge` hiện là cờ do backend tính toán từ tuổi của child để hỗ trợ UI gợi ý, không phải giá trị FE nhập vào.
- Status: 200, 404 if not found/unauthorized

### POST /api/child

- Request: `CreateChildRequest`
  - `Name: string`
  - `Age: int`
  - `AvatarUrl?: string`
- Response: 201 Created, `ApiResponse<ChildResponse>`

### PUT /api/child/{childId}

- Request: `UpdateChildRequest`
  - `Name: string`
  - `Age: int`
  - `AvatarUrl?: string`
- Response: `ApiResponse<ChildResponse>`

### DELETE /api/child/{childId}

- Soft delete (IsDeleted = true)
- Response: `ApiResponse<object>`

### PATCH /api/child/{childId}/restore

- Restore soft-deleted child
- Response: `ApiResponse<object>`

### DELETE /api/child/{childId}/hard

- Auth: Admin role required
- Hard delete permanent
- Response: `ApiResponse<object>`

---

## 3. Topics (`TopicController`)

### GET /api/topic

- Query: `pageNumber`, `pageSize`
- Auth: AllowAnonymous
- Response: `ApiResponse<PagedResponse<TopicSummaryResponse>>`
- `TopicSummaryResponse`:
  - `Id: Guid`
  - `Name: string`
  - `IconUrl?: string`
  - `OrderIndex: int`
  - `TotalLessons: int`

### GET /api/topic/{topicId}

- Auth: AllowAnonymous
- Response: `ApiResponse<TopicResponse>`
- `TopicResponse`:
  - `Id: Guid`
  - `Name: string`
  - `Description?: string`
  - `IconUrl?: string`
  - `OrderIndex: int`
  - `IsActive: bool`
  - `TotalLessons: int`
  - `CreatedAt: DateTime`

### POST /api/topic

- Auth: Admin
- Request: `CreateTopicRequest`
  - `Name: string`
  - `Description?: string`
  - `IconUrl?: string`
  - `OrderIndex: int`
- Response: 201 Created `ApiResponse<TopicResponse>`

### PUT /api/topic/{topicId}

- Auth: Admin
- Request: `UpdateTopicRequest`
  - `Name: string`
  - `Description?: string`
  - `IconUrl?: string`
  - `OrderIndex: int`
  - `IsActive: bool`
- Response: `ApiResponse<TopicResponse>`

### DELETE /api/topic/{topicId}

- Auth: Admin (soft-delete)
- Response: `ApiResponse<object>`

### PATCH /api/topic/{topicId}/restore

- Auth: Admin
- Response: `ApiResponse<object>`

### DELETE /api/topic/{topicId}/hard

- Auth: Admin
- Response: `ApiResponse<object>`

---

## 4. Lessons (`LessonController`)

### GET /api/lesson/all

- Query: `pageNumber`, `pageSize`
- Auth: AllowAnonymous (Admin will see unpublished)
- Response: `ApiResponse<PagedResponse<LessonSummaryResponse>>`
- `LessonSummaryResponse`:
  - `Id: Guid`
  - `Title: string`
  - `LessonType: string`
  - `Difficulty: string`
  - `SkillFocus: string`
  - `DurationSeconds: int`
  - `ThumbnailUrl?: string`
  - `OrderIndex: int`
  - `IsPublished: bool`

### GET /api/lesson/topic/{topicId}

- Query: `pageNumber`, `pageSize`
- Auth: AllowAnonymous
- Response: `ApiResponse<PagedResponse<LessonSummaryResponse>>`
- `LessonSummaryResponse`:
  - `Id: Guid`
  - `Title: string`
  - `LessonType: string`
  - `Difficulty: string`
  - `SkillFocus: string`
  - `DurationSeconds: int`
  - `ThumbnailUrl?: string`
  - `OrderIndex: int`
  - `IsPublished: bool`

### GET /api/lesson/{lessonId}

- Auth: AllowAnonymous
- Response: `ApiResponse<LessonResponse>`
- `LessonResponse`:
  - `Id: Guid`
  - `Title: string`
  - `Description?: string`
  - `LessonType: string`
  - `Difficulty: string`
  - `SkillFocus: string`
  - `DurationSeconds: int`
  - `ThumbnailUrl?: string`
  - `AudioUrl?: string`
  - `VideoUrl?: string`
  - `ContentJson?: string`
  - `OrderIndex: int`
  - `IsPublished: bool`
  - `TopicId: Guid`
  - `TopicName: string`
  - `TotalVocabularies: int`
  - `CreatedAt: DateTime`

### POST /api/lesson

- Auth: Admin
- Request: `CreateLessonRequest`
  - `Title: string`
  - `Description?: string`
  - `LessonType: string`
  - `Difficulty: string`
  - `SkillFocus: string`
  - `DurationSeconds: int`
  - `ThumbnailUrl?: string`
  - `AudioUrl?: string`
  - `VideoUrl?: string`
  - `ContentJson?: string`
  - `OrderIndex: int`
  - `TopicId: Guid`
- Response: 201 Created `ApiResponse<LessonResponse>`

### PUT /api/lesson/{lessonId}

- Auth: Admin
- Request: `UpdateLessonRequest`
  - `Title: string`
  - `Description?: string`
  - `LessonType: string`
  - `Difficulty: string`
  - `SkillFocus: string`
  - `DurationSeconds: int`
  - `ThumbnailUrl?: string`
  - `AudioUrl?: string`
  - `VideoUrl?: string`
  - `ContentJson?: string`
  - `OrderIndex: int`
  - `IsPublished: bool`
- Response: `ApiResponse<LessonResponse>`

### DELETE /api/lesson/{lessonId}

- Auth: Admin (soft delete)
- Response: `ApiResponse<object>`

### PATCH /api/lesson/{lessonId}/publish

- Auth: Admin
- Response: `ApiResponse<object>`

### PATCH /api/lesson/{lessonId}/unpublish

- Auth: Admin
- Response: `ApiResponse<object>`

### PATCH /api/lesson/{lessonId}/restore

- Auth: Admin
- Response: `ApiResponse<object>`

### DELETE /api/lesson/{lessonId}/hard

- Auth: Admin
- Response: `ApiResponse<object>`

---

## 5. Vocabularies (`VocabularyController`)

### GET /api/vocabulary/paged

- Auth: Admin
- Query: `page`, `pageSize`, `lessonId` (optional)
- Response: `ApiResponse<PagedResponse<VocabularyResponse>>`
- `VocabularyResponse`:
  - `Id: Guid`
  - `Word: string`
  - `Meaning: string`
  - `PhoneticText?: string`
  - `AudioUrl?: string`
  - `ImageUrl?: string`
  - `ExampleSentence?: string`
  - `OrderIndex: int`
  - `LessonId: Guid`
  - `LessonTitle: string`
  - `CreatedAt: DateTime`

### GET /api/vocabulary/all

- Auth: Admin
- Response: `ApiResponse<PagedResponse<VocabularyResponse>>`

### GET /api/vocabulary/search

- Auth: Admin
- Query: `keyword`, `lessonId`, `pageNumber`, `pageSize`
- Response: `ApiResponse<PagedResponse<VocabularyResponse>>`

### GET /api/vocabulary/lesson/{lessonId}

- Auth: AllowAnonymous
- Query: `pageNumber`, `pageSize`
- Response: `ApiResponse<PagedResponse<VocabularyResponse>>`
- `VocabularyResponse`:
  - `Id: Guid`
  - `Word: string`
  - `Meaning: string`
  - `PhoneticText?: string`
  - `AudioUrl?: string`
  - `ImageUrl?: string`
  - `ExampleSentence?: string`
  - `OrderIndex: int`
  - `LessonId: Guid`
  - `LessonTitle: string`
  - `CreatedAt: DateTime`

### GET /api/vocabulary/{vocabId}

- Auth: AllowAnonymous
- Response: `ApiResponse<VocabularyResponse>`

### POST /api/vocabulary

- Auth: Admin
- Request: `CreateVocabularyRequest`
  - `Word: string`
  - `Meaning: string`
  - `PhoneticText?: string`
  - `AudioUrl?: string`
  - `ImageUrl?: string`
  - `ExampleSentence?: string`
  - `OrderIndex: int`
  - `LessonId: Guid`
- Response: 201 Created `ApiResponse<VocabularyResponse>`

### PUT /api/vocabulary/{vocabId}

- Auth: Admin
- Request: `UpdateVocabularyRequest`
  - `Word: string`
  - `Meaning: string`
  - `PhoneticText?: string`
  - `AudioUrl?: string`
  - `ImageUrl?: string`
  - `ExampleSentence?: string`
  - `OrderIndex: int`
- Response: `ApiResponse<VocabularyResponse>`

### DELETE /api/vocabulary/{vocabId}

- Auth: Admin (soft)
- Response: `ApiResponse<object>`

### PATCH /api/vocabulary/{vocabId}/restore

- Auth: Admin
- Response: `ApiResponse<object>`

### DELETE /api/vocabulary/{vocabId}/hard

- Auth: Admin
- Response: `ApiResponse<object>`

---

## 6. Progress (`ProgressController`)

All endpoints require Bearer auth.

### POST /api/progress/submit

- Request: `SubmitProgressRequest`
  - `ChildId: Guid`
  - `LessonId: Guid`
  - `ScorePercent: int` (0-100)
  - `TimeSpentSeconds: int`
- Response: `ApiResponse<ProgressResponse>`
- `ProgressResponse`:
  - `Id: Guid`
  - `ChildId: Guid`
  - `ChildName: string`
  - `LessonId: Guid`
  - `LessonTitle: string`
  - `IsCompleted: bool`
  - `StarsEarned: int`
  - `ScorePercent: int`
  - `TimeSpentSeconds: int`
  - `AttemptCount: int`
  - `CompletedAt?: DateTime`
  - `CreatedAt: DateTime`
  - `NewAchievements: List<AchievementResponse>`
- Status: 200

### GET /api/progress/child/{childId}

- Query: `pageNumber`, `pageSize`
- Response: `ApiResponse<PagedResponse<ProgressResponse>>`

### GET /api/progress/child/{childId}/lesson/{lessonId}

- Response: `ApiResponse<ProgressResponse?>`

### GET /api/progress/child/{childId}/summary

- Response: `ApiResponse<ChildProgressSummary>`
- `ChildProgressSummary`:
  - `ChildId: Guid`
  - `ChildName: string`
  - `TotalLessonsCompleted: int`
  - `TotalStars: int`
  - `CurrentStreakDays: int`
  - `LastLessonAt?: DateTime`
  - `TopicProgresses: List<TopicProgressItem>`
- `TopicProgressItem`:
  - `TopicId: Guid`
  - `TopicName: string`
  - `TotalLessons: int`
  - `CompletedLessons: int`
  - `ProgressPercent: int`

### GET /api/progress/child/{childId}/recent-activities

- Query: `pageNumber`, `pageSize`
- Response: `ApiResponse<PagedResponse<ProgressResponse>>`

### GET /api/progress/child/{childId}/completed-lessons

- Query: `pageNumber`, `pageSize`
- Response: `ApiResponse<PagedResponse<ProgressResponse>>`

---

## 7. Pronunciation (`PronunciationController`)

All endpoints require Bearer auth.

### POST /api/pronunciation/submit

- Content-Type: `multipart/form-data`
- Form fields: `VocabularyId` (Guid), optional `LessonId` (Guid), `AudioFile` (IFormFile)
- Lưu ý: FE phải truyền đúng key file là `AudioFile` trong form-data.
- Constraints: WAV only, max ~10MB (enforced by service)
- Response: `ApiResponse<PronunciationScoreResponse>`
- `PronunciationScoreResponse`:
  - `Id: Guid`
  - `VocabularyId: Guid`
  - `Word: string`
  - `AccuracyScore: int`
  - `FluencyScore: int`
  - `CompletenessScore: int`
  - `OverallScore: int`
  - `IsPassed: bool`
  - `Feedback: string`
  - `AudioStorageUrl: string`
  - `CreatedAt: DateTime`
- Errors: 400 for validation (AppException), 500 for internal errors

### GET /api/pronunciation/vocabulary/{vocabularyId}

- Response: `ApiResponse<PronunciationHistoryResponse>`
- `PronunciationHistoryResponse`:
  - `VocabularyId: Guid`
  - `Word: string`
  - `AttemptCount: int`
  - `BestScore: int`
  - `LastAttemptScore: int`
  - `LastAttempt?: PronunciationScoreResponse`
- Status: 200 / 404

### GET /api/pronunciation/history

- Query: `pageNumber`, `pageSize`
- Response: `ApiResponse<PagedPronunciationResponse>`
- `PagedPronunciationResponse`:
  - `Items: List<PronunciationScoreResponse>`
  - `PageNumber: int`
  - `PageSize: int`
  - `TotalCount: int`
  - `TotalPages: int`
  - `HasNextPage: bool`
  - `HasPreviousPage: bool`

---

## 8. Text-to-Speech (`TextToSpeechController`)

All endpoints require Bearer auth except `GET /api/tts/voices`.

### POST /api/tts/synthesize

- Request: `TextToSpeechSynthesizeRequest`
  - `Voice: string`
  - `Rate: int`
  - `Pitch: int`
  - `Text: string`
- Response: `ApiResponse<TextToSpeechGeneratedResponse>`
- `TextToSpeechGeneratedResponse`:
  - `AudioUrl: string`
  - `FileName: string`
  - `VoiceCode: string`
  - `VoiceName: string`
  - `IsCached: bool`

### POST /api/tts/lesson/{lessonId}

- Request: `TextToSpeechOptionsRequest`
  - `Voice: string`
  - `Rate: int`
  - `Pitch: int`
- Response: `ApiResponse<TextToSpeechGeneratedResponse>`

### GET /api/tts/voices

- Auth: AllowAnonymous
- Response: `ApiResponse<List<TextToSpeechVoiceResponse>>`
- `TextToSpeechVoiceResponse`:
  - `Code: string`
  - `DisplayName: string`
  - `AzureVoiceName: string`
  - `Locale: string`
  - `Gender: string`
  - `IsDefault: bool`

---

## 9. Achievements (`AchievementController`)

All endpoints require Bearer auth; admin-only endpoints have `Authorize(Roles = "Admin")`.

### GET /api/achievement/child/{childId}

- Query: `pageNumber`, `pageSize`
- Response: `ApiResponse<PagedResponse<AchievementResponse>>`
- `AchievementResponse`:
  - `Id: Guid`
  - `Name: string`
  - `Description?: string`
  - `BadgeUrl?: string`
  - `AchievementType: string`
  - `Threshold: int`
  - `EarnedAt: DateTime`

### GET /api/achievement/definitions

- Auth: Admin
- Query: `pageNumber`, `pageSize`
- Response: `ApiResponse<PagedResponse<AchievementDefinitionResponse>>`
- `AchievementDefinitionResponse`:
  - `Id: Guid`
  - `Type: string`
  - `Threshold: int`
  - `Name: string`
  - `Description?: string`
  - `BadgeUrl?: string`
  - `OrderIndex: int`
  - `IsActive: bool`

### GET /api/achievement/definitions/{id}

- Auth: Admin
- Response: `ApiResponse<AchievementDefinitionResponse>` or 404

### POST /api/achievement/definitions

- Auth: Admin
- Request: `CreateAchievementDefinitionRequest`
  - `Type: string`
  - `Threshold: int`
  - `Name: string`
  - `Description?: string`
  - `BadgeUrl?: string`
  - `OrderIndex: int`
- Response: 201 Created `ApiResponse<AchievementDefinitionResponse>`

### PUT /api/achievement/definitions/{id}

- Auth: Admin
- Request: `UpdateAchievementDefinitionRequest`
  - `Type: string`
  - `Threshold: int`
  - `Name: string`
  - `Description?: string`
  - `BadgeUrl?: string`
  - `OrderIndex: int`
  - `IsActive: bool`
- Response: `ApiResponse<AchievementDefinitionResponse>` or 404

### DELETE /api/achievement/definitions/{id}

- Auth: Admin (soft)
- Response: `ApiResponse<bool>` (true if deleted) or 404

### PATCH /api/achievement/definitions/{id}/restore

- Auth: Admin
- Response: `ApiResponse<bool>` (true if restored) or 404

### DELETE /api/achievement/definitions/{id}/hard

- Auth: Admin
- Response: `ApiResponse<bool>` (true if hard deleted) or 400/404

---

## Error handling & validation

- Controllers often catch `AppException` and return `400 BadRequest` with `ApiResponse.Fail(message)`.
- 401 Unauthorized returned when JWT missing or invalid.
- 403 Forbidden for role-restricted endpoints.
- 404 NotFound when entity not found.
- Multipart upload endpoints enforce file type and size in business layer.

---

## Notes & next steps

- DTO definitions are in `KIDIO.Business/DTOs/*` — use them as canonical schema shapes.
- If bạn muốn, tôi có thể:
  - Thêm ví dụ JSON request/response cho từng endpoint;
  - Nhập nội dung này vào `README.md` hoặc mở thành OpenAPI spec (Swagger) file;
  - Tạo Postman collection tự động.
