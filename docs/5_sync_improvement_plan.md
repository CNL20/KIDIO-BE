# 5. KẾ HOẠCH CẢI THIỆN ĐỒNG BỘ HÓA HỆ THỐNG (SYNC IMPROVEMENT PLAN)

> **Ngày soạn:** 23/06/2026  
> **Phiên bản:** 1.0  
> **Trạng thái:** Chờ implement

Tài liệu này mô tả chi tiết các lỗi đồng bộ hóa dữ liệu giữa Frontend (Flutter)
và Backend (.NET), kèm theo kế hoạch implement theo **Hướng B** (persist lên server).

---

## A. BỐI CẢNH & PHÂN TÍCH VẤN ĐỀ

Qua audit toàn bộ Provider layer của Flutter app, phát hiện **4 vấn đề**
theo thứ tự ưu tiên:

| # | Mức độ | Mô tả ngắn | Hướng fix |
|---|---|---|---|
| 1 | 🔴 HIGH | Quest reward stars không persist lên server | Thêm API endpoint |
| 2 | 🔴 HIGH | Quest phát âm không cập nhật real-time trên UI | Sửa Provider watcher |
| 3 | 🟡 MEDIUM | `_claimReward` thiếu `setState` | Sửa UI logic |
| 4 | 🟢 LOW | `scorePercent` hardcode = 100 | Tính score thực |

---

## B. VẤN ĐỀ 1: QUEST REWARD — STARS KHÔNG PERSIST (🔴 CRITICAL)

### Mô tả hiện trạng

Khi bé hoàn thành một nhiệm vụ hàng ngày (Quest) và bấm "NHẬN":

```
[Quest Screen] _claimReward()
  → child.totalStars + quest.reward  (chỉ trong RAM)
  → childProvider.selectChild(updatedChild)  (chỉ cập nhật Provider local)
  → Lưu Hive: 'quest_{id}_claimed_{childId}_{date}' = true
```

**Kết quả:** Backend KHÔNG biết về stars này. Khi `refreshSelectedChild()` 
được gọi sau đó (ví dụ: sau khi submit lesson), server trả về `TotalStars`
chưa cộng quest → UI bị reset về số cũ.

### Giải pháp: Thêm API endpoint `POST /api/child/{childId}/add-stars`

---

### B.1 — Backend Changes

#### B.1.1 — DTO mới

**File:** `KIDIO.Business/DTOs/Child/ChildDtos.cs`

```csharp
// Thêm vào file ChildDtos.cs hiện có
public record AddStarsRequest(
    int Stars,          // Số sao cộng thêm (> 0)
    string Reason       // Lý do: "quest_daily", "achievement", v.v.
);

public record AddStarsResponse(
    Guid ChildId,
    string ChildName,
    int StarsAdded,
    int TotalStars      // Tổng stars mới sau khi cộng
);
```

#### B.1.2 — Interface

**File:** `KIDIO.Business/Interfaces/IChildService.cs`

```csharp
// Thêm method vào interface
Task<AddStarsResponse> AddStarsAsync(
    Guid childId, 
    Guid parentId, 
    AddStarsRequest request, 
    CancellationToken ct = default);
```

#### B.1.3 — Service Implementation

**File:** `KIDIO.Business/Services/ChildService.cs`

```csharp
public async Task<AddStarsResponse> AddStarsAsync(
    Guid childId, Guid parentId, AddStarsRequest request, CancellationToken ct = default)
{
    if (request.Stars <= 0)
        throw new AppException("Stars to add must be greater than 0.");

    var child = await _uow.Children.GetByIdAsync(childId, ct)
        ?? throw new NotFoundException("Child");

    if (child.ParentId != parentId)
        throw new ForbiddenException("You do not have access to this child profile.");

    child.TotalStars += request.Stars;
    _uow.Children.Update(child);
    await _uow.SaveChangesAsync(ct);

    return new AddStarsResponse(
        ChildId:    child.Id,
        ChildName:  child.Name,
        StarsAdded: request.Stars,
        TotalStars: child.TotalStars
    );
}
```

#### B.1.4 — Controller Endpoint

**File:** `KIDIO.API/Controllers/ChildController.cs`

```csharp
/// <summary>
/// Cộng thêm Stars vào hồ sơ của Child (dùng cho Quest reward, bonus, v.v.)
/// </summary>
[HttpPost("{childId:guid}/add-stars")]
public async Task<ActionResult<ApiResponse<AddStarsResponse>>> AddStars(
    Guid childId,
    [FromBody] AddStarsRequest request,
    CancellationToken ct)
{
    var result = await _childService.AddStarsAsync(
        childId, GetCurrentUserId(), request, ct);

    return Ok(ApiResponse<AddStarsResponse>.Ok(result, "Stars added successfully."));
}
```

---

### B.2 — Frontend Changes

#### B.2.1 — ChildRepository

**File:** `lib/repositories/child_repository.dart`

Thêm method:

```dart
Future<Map<String, dynamic>> addStars({
  required String childId,
  required int stars,
  required String reason,
}) async {
  final response = await _apiClient.dio.post(
    'Child/$childId/add-stars',
    data: {'stars': stars, 'reason': reason},
  );
  return response.data['data'] as Map<String, dynamic>;
}
```

#### B.2.2 — ChildProvider

**File:** `lib/providers/child_provider.dart`

Thêm method `addQuestStars`:

```dart
Future<bool> addQuestStars({
  required String childId,
  required int stars,
  required String reason,
}) async {
  try {
    final result = await _repository.addStars(
      childId: childId,
      stars: stars,
      reason: reason,
    );
    // Cập nhật selectedChild với totalStars mới từ server
    if (_selectedChild != null) {
      final newTotal = result['totalStars'] as int;
      _selectedChild = Child(
        id: _selectedChild!.id,
        name: _selectedChild!.name,
        age: _selectedChild!.age,
        avatarUrl: _selectedChild!.avatarUrl,
        totalStars: newTotal,              // ← từ server, không tính local
        currentStreakDays: _selectedChild!.currentStreakDays,
        lastLessonAt: _selectedChild!.lastLessonAt,
      );
      notifyListeners();
    }
    return true;
  } catch (e) {
    debugPrint('Error adding quest stars: $e');
    return false;
  }
}
```

#### B.2.3 — QuestScreen: cập nhật `_claimReward`

**File:** `lib/screens/quest_screen.dart`

```dart
Future<void> _claimReward(Quest quest) async {
  if (!quest.isDone || quest.isClaimed) return;

  final childProvider = context.read<ChildProvider>();
  final child = childProvider.selectedChild;
  if (child == null) return;

  final childId = child.id;
  final todayStr = DateTime.now().toIso8601String().substring(0, 10);
  final box = Hive.box('kidio_cache');

  // 1. Optimistic local update — phản hồi ngay cho UI
  await box.put('quest_${quest.id}_claimed_${childId}_$todayStr', true);
  if (mounted) setState(() {});   // ← FIX Bug #3: trigger rebuild claim status

  // 2. Persist lên server (Hướng B)
  final success = await childProvider.addQuestStars(
    childId: childId,
    stars: quest.reward,
    reason: 'quest_daily_${quest.id}',
  );

  if (mounted) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(success
            ? '🎉 Nhận thành công ${quest.reward} ⭐! Tổng sao: ${childProvider.selectedChild?.totalStars ?? 0} ⭐'
            : '⭐ Đã lưu offline, sẽ đồng bộ khi có mạng!'),
        backgroundColor: success ? const Color(0xFF03A566) : Colors.orange,
        behavior: SnackBarBehavior.floating,
      ),
    );
  }
}
```

---

## C. VẤN ĐỀ 2: QUEST PHÁT ÂM KHÔNG REAL-TIME (🔴 HIGH)

### Mô tả hiện trạng

`PronunciationProvider.submitPronunciation()` cập nhật Hive và gọi
`notifyListeners()` — nhưng `QuestScreen` **không** `watch<PronunciationProvider>()`.

```dart
// quest_screen.dart - build()
final progressProvider = context.watch<ProgressProvider>();  // ✅
final childProvider    = context.watch<ChildProvider>();      // ✅
// PronunciationProvider: ❌ THIẾU
final pronCount = box.get('daily_pron_count_...');           // đọc Hive trực tiếp
```

### Giải pháp: Expose counter từ PronunciationProvider

#### C.1 — PronunciationProvider

**File:** `lib/providers/pronunciation_provider.dart`

Thêm state `_dailyPronCount` và getter:

```dart
// Thêm field
int _dailyPronCount = 0;
int get dailyPronCount => _dailyPronCount;

// Thêm method để load từ Hive khi init
void loadDailyCount(String childId) {
  final box = Hive.box('kidio_cache');
  final todayStr = DateTime.now().toIso8601String().substring(0, 10);
  _dailyPronCount = box.get('daily_pron_count_${childId}_$todayStr', defaultValue: 0) as int;
  notifyListeners();
}

// Trong submitPronunciation(), sau khi tăng Hive counter:
_dailyPronCount = currentCount + 1;   // ← thêm dòng này
notifyListeners();                    // ← đã có sẵn
```

#### C.2 — QuestScreen

**File:** `lib/screens/quest_screen.dart`

```dart
// Thêm watcher
final pronProvider = context.watch<PronunciationProvider>();

// Dùng counter từ Provider thay vì đọc Hive trực tiếp
final pronCount = pronProvider.dailyPronCount;   // ← real-time
```

Gọi `loadDailyCount` khi child thay đổi (trong `build` hoặc `initState`):

```dart
@override
void initState() {
  super.initState();
  WidgetsBinding.instance.addPostFrameCallback((_) {
    final childId = context.read<ChildProvider>().selectedChild?.id;
    if (childId != null) {
      context.read<PronunciationProvider>().loadDailyCount(childId);
    }
  });
}
```

---

## D. VẤN ĐỀ 3: MISSING `setState` TRONG `_claimReward` (🟡 MEDIUM)

Đã được xử lý trong phần C.2.3 ở trên — thêm `if (mounted) setState(() {})` 
ngay sau khi lưu Hive claim status, trước khi gọi API.

---

## E. VẤN ĐỀ 4: `scorePercent` HARDCODE = 100 (🟢 LOW)

### Mô tả

`lesson_detail_screen.dart` luôn gửi `scorePercent: 100` bất kể
kết quả quiz/boss thực tế của bé.

### Giải pháp (tùy chọn, implement sau)

1. Các màn hình quiz/boss trả về `int score` qua `Navigator.pop(context, score)`
2. `LessonDetailScreen` collect kết quả từ từng activity
3. Tính trung bình: `finalScore = (quizScore + bossScore + pronScore) / 3`
4. Submit score thực lên server

> **Lưu ý:** Đây là tính năng non-critical. Bé vẫn học và nhận đủ sao ngay
> cả với score = 100 hardcode. Implement sau khi các bug 1-3 đã ổn định.

---

## F. THỨ TỰ IMPLEMENT

```
Sprint 1 (Bug fixes — 1-2 ngày)
├── [BE] B.1: Thêm DTO + Service + Endpoint add-stars
├── [FE] B.2: Thêm method addStars vào ChildRepository
├── [FE] B.2: Thêm addQuestStars vào ChildProvider
├── [FE] B.2: Cập nhật _claimReward (Hướng B + setState fix)
├── [FE] C.1: Thêm dailyPronCount vào PronunciationProvider
└── [FE] C.2: QuestScreen watch PronunciationProvider

Sprint 2 (Enhancement — tuỳ chọn)
└── [FE] E: Tính score thực từ quiz/boss results
```

---

## G. API ENDPOINT MỚI — TÓM TẮT

| Method | Route | Auth | Mô tả |
|---|---|---|---|
| `POST` | `/api/child/{childId}/add-stars` | Bearer (Parent) | Cộng Stars cho Child từ Quest reward |

**Request Body:**
```json
{
  "stars": 15,
  "reason": "quest_daily_3"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Stars added successfully.",
  "data": {
    "childId": "...",
    "childName": "Bé An",
    "starsAdded": 15,
    "totalStars": 145
  }
}
```

---

> *Sau khi implement xong, cập nhật `4_api_reference.md` để thêm endpoint mới vào Section 2 (Child Management).*
