using KIDIO.Business.DTOs.Achievement;
using KIDIO.Business.DTOs.Progress;
using KIDIO.Business.Extensions;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class ProgressService : IProgressService
{
    private readonly IUnitOfWork _uow;
    private readonly IAchievementService _achievementService;

    public ProgressService(IUnitOfWork uow, IAchievementService achievementService)
    {
        _uow = uow;
        _achievementService = achievementService;
    }

    public async Task<ProgressResponse> SubmitProgressAsync(
        Guid parentId, SubmitProgressRequest request, CancellationToken ct = default)
    {
        // Verify ownership
        var child = await _uow.Children.GetByIdAsync(request.ChildId, ct)
            ?? throw new NotFoundException("Child");

        if (child.ParentId != parentId)
            throw new ForbiddenException("You do not have access to this child profile.");

        // Verify lesson tồn tại và đã published
        var lesson = await _uow.Lessons.GetByIdAsync(request.LessonId, ct)
            ?? throw new NotFoundException("Lesson");

        if (!lesson.IsPublished)
            throw new AppException("This lesson is not available.");

        // Validate score
        if (request.ScorePercent < 0 || request.ScorePercent > 100)
            throw new AppException("Score must be between 0 and 100.");

        // Tính sao dựa trên điểm
        var stars = CalculateStars(request.ScorePercent);
        var isCompleted = request.ScorePercent >= 60; // >= 60% là pass

        // Tìm progress cũ nếu có (upsert)
        var existing = await _uow.LessonProgresses.FirstOrDefaultAsync(
            p => p.ChildId == request.ChildId && p.LessonId == request.LessonId, ct);

        if (existing is not null)
        {
            // Chỉ update nếu điểm cao hơn lần trước
            existing.AttemptCount++;
            existing.TimeSpentSeconds += request.TimeSpentSeconds;

            if (request.ScorePercent >= existing.ScorePercent)
            {
                existing.ScorePercent = request.ScorePercent;
                existing.StarsEarned = stars;
            }

            if (isCompleted && !existing.IsCompleted)
            {
                existing.IsCompleted = true;
                existing.CompletedAt = DateTime.UtcNow;
            }

            _uow.LessonProgresses.Update(existing);
        }
        else
        {
            existing = new LessonProgress
            {
                ChildId = request.ChildId,
                LessonId = request.LessonId,
                IsCompleted = isCompleted,
                StarsEarned = stars,
                ScorePercent = request.ScorePercent,
                TimeSpentSeconds = request.TimeSpentSeconds,
                CompletedAt = isCompleted ? DateTime.UtcNow : null,
                AttemptCount = 1
            };

            await _uow.LessonProgresses.AddAsync(existing, ct);
        }

        // Cập nhật TotalStars và Streak cho child
        await UpdateChildStatsAsync(child, isCompleted, stars, existing, ct);

        await _uow.SaveChangesAsync(ct);

        var achievementResult = await _achievementService.CheckAndUnlockAsync(child.Id, ct);

        return await BuildProgressResponseAsync(existing, achievementResult.NewAchievements, ct);
    }

    public async Task<PagedResponse<ProgressResponse>> GetProgressByChildAsync(
        Guid childId, Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        await VerifyChildOwnershipAsync(childId, parentId, ct);

        var query = _uow.LessonProgresses.Query()
            .Include(p => p.Child)
            .Include(p => p.Lesson)
            .Where(p => p.ChildId == childId)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Select(p => new ProgressResponse(
                p.Id,
                p.ChildId,
                p.Child.Name,
                p.LessonId,
                p.Lesson.Title,
                p.IsCompleted,
                p.StarsEarned,
                p.ScorePercent,
                p.TimeSpentSeconds,
                p.AttemptCount,
                p.CompletedAt,
                p.CreatedAt,
                new List<AchievementResponse>())) ;

        return await query.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<ProgressResponse?> GetLessonProgressAsync(
        Guid childId, Guid lessonId, Guid parentId, CancellationToken ct = default)
    {
        await VerifyChildOwnershipAsync(childId, parentId, ct);

        var progress = await _uow.LessonProgresses.Query()
            .Include(p => p.Child)
            .Include(p => p.Lesson)
            .FirstOrDefaultAsync(
                p => p.ChildId == childId && p.LessonId == lessonId, ct);

        return progress is null ? null : MapToResponse(progress);
    }

    public async Task<ChildProgressSummary> GetChildSummaryAsync(
        Guid childId, Guid parentId, CancellationToken ct = default)
    {
        var child = await VerifyChildOwnershipAsync(childId, parentId, ct);

        // Lấy tất cả progress của child
        var progresses = await _uow.LessonProgresses.Query()
            .Where(p => p.ChildId == childId && p.IsCompleted)
            .ToListAsync(ct);

        // Lấy tất cả topic + lesson để tính %
        var topics = await _uow.Topics.Query()
            .Where(t => t.IsActive)
            .Include(t => t.Lessons.Where(l => l.IsPublished && !l.IsDeleted))
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(ct);

        var completedLessonIds = progresses.Select(p => p.LessonId).ToHashSet();

        var topicProgresses = topics.Select(t =>
        {
            var total = t.Lessons.Count;
            var completed = t.Lessons.Count(l => completedLessonIds.Contains(l.Id));
            var percent = total == 0 ? 0 : (int)Math.Round((double)completed / total * 100);

            return new TopicProgressItem(
                TopicId: t.Id,
                TopicName: t.Name,
                TotalLessons: total,
                CompletedLessons: completed,
                ProgressPercent: percent
            );
        }).ToList();

        return new ChildProgressSummary(
            ChildId: child.Id,
            ChildName: child.Name,
            TotalLessonsCompleted: progresses.Count,
            TotalStars: child.TotalStars,
            CurrentStreakDays: child.CurrentStreakDays,
            LastLessonAt: child.LastLessonAt,
            TopicProgresses: topicProgresses
        );
    }

    public async Task<PagedResponse<ProgressResponse>> GetRecentActivitiesAsync(
        Guid childId, Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        await VerifyChildOwnershipAsync(childId, parentId, ct);

        var query = _uow.LessonProgresses.Query()
            .Include(p => p.Child)
            .Include(p => p.Lesson)
            .Where(p => p.ChildId == childId)
            .OrderByDescending(p => p.UpdatedAt ?? p.CompletedAt ?? p.CreatedAt)
            .Select(p => new ProgressResponse(
                p.Id,
                p.ChildId,
                p.Child.Name,
                p.LessonId,
                p.Lesson.Title,
                p.IsCompleted,
                p.StarsEarned,
                p.ScorePercent,
                p.TimeSpentSeconds,
                p.AttemptCount,
                p.CompletedAt,
                p.CreatedAt,
                new List<AchievementResponse>())) ;

        return await query.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<PagedResponse<ProgressResponse>> GetCompletedLessonsAsync(
        Guid childId, Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        await VerifyChildOwnershipAsync(childId, parentId, ct);

        var query = _uow.LessonProgresses.Query()
            .Include(p => p.Child)
            .Include(p => p.Lesson)
            .Where(p => p.ChildId == childId && p.IsCompleted)
            .OrderByDescending(p => p.CompletedAt ?? p.UpdatedAt ?? p.CreatedAt)
            .Select(p => new ProgressResponse(
                p.Id,
                p.ChildId,
                p.Child.Name,
                p.LessonId,
                p.Lesson.Title,
                p.IsCompleted,
                p.StarsEarned,
                p.ScorePercent,
                p.TimeSpentSeconds,
                p.AttemptCount,
                p.CompletedAt,
                p.CreatedAt,
                new List<AchievementResponse>())) ;

        return await query.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    // ── Helpers ─────────────────────────────────────────────

    private static int CalculateStars(int scorePercent) => scorePercent switch
    {
        >= 90 => 3,
        >= 70 => 2,
        >= 60 => 1,
        _ => 0
    };

    private async Task UpdateChildStatsAsync(
        Child child,
        bool isCompleted,
        int newStars,
        LessonProgress progress,
        CancellationToken ct)
    {
        if (!isCompleted) return;

        // Cộng sao — chỉ cộng phần tăng thêm so với lần trước
        var oldStars = progress.StarsEarned;
        var starsDelta = Math.Max(0, newStars - oldStars);
        child.TotalStars += starsDelta;

        // Cập nhật streak
        var today = DateTime.UtcNow.Date;

        if (child.LastLessonAt.HasValue)
        {
            var lastDate = child.LastLessonAt.Value.Date;

            if (lastDate == today)
            {
                // Hôm nay đã học rồi, không tăng streak
            }
            else if (lastDate == today.AddDays(-1))
            {
                // Ngày liên tiếp → tăng streak
                child.CurrentStreakDays++;
            }
            else
            {
                // Bị gián đoạn → reset streak
                child.CurrentStreakDays = 1;
            }
        }
        else
        {
            // Lần đầu học
            child.CurrentStreakDays = 1;
        }

        child.LastLessonAt = DateTime.UtcNow;
        _uow.Children.Update(child);
    }

    private async Task<Child> VerifyChildOwnershipAsync(
        Guid childId, Guid parentId, CancellationToken ct)
    {
        var child = await _uow.Children.GetByIdAsync(childId, ct)
            ?? throw new NotFoundException("Child");

        if (child.ParentId != parentId)
            throw new ForbiddenException("You do not have access to this child profile.");

        return child;
    }

    private async Task<ProgressResponse> BuildProgressResponseAsync(
        LessonProgress p,
        List<AchievementResponse> newAchievements,
        CancellationToken ct)
    {
        var progress = await _uow.LessonProgresses.Query()
            .Include(x => x.Child)
            .Include(x => x.Lesson)
            .FirstOrDefaultAsync(x => x.Id == p.Id, ct)
            ?? throw new NotFoundException("Progress");

        return MapToResponse(progress, newAchievements);
    }

    private static ProgressResponse MapToResponse(
        LessonProgress p,
        List<AchievementResponse>? newAchievements = null) => new(
        Id: p.Id,
        ChildId: p.ChildId,
        ChildName: p.Child?.Name ?? string.Empty,
        LessonId: p.LessonId,
        LessonTitle: p.Lesson?.Title ?? string.Empty,
        IsCompleted: p.IsCompleted,
        StarsEarned: p.StarsEarned,
        ScorePercent: p.ScorePercent,
        TimeSpentSeconds: p.TimeSpentSeconds,
        AttemptCount: p.AttemptCount,
        CompletedAt: p.CompletedAt,
        CreatedAt: p.CreatedAt,
        NewAchievements: newAchievements ?? []
    );
}