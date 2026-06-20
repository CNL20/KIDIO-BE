using KIDIO.Business.DTOs.Dashboard;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class ParentDashboardService : IParentDashboardService
{
    private readonly IUnitOfWork _uow;

    public ParentDashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ParentDashboardOverviewResponse> GetOverviewAsync(
        Guid parentId,
        int weeks = 4,
        CancellationToken ct = default)
    {
        if (weeks < 1) weeks = 4;
        if (weeks > 12) weeks = 12;

        var parent = await _uow.Users.GetByIdAsync(parentId, ct)
            ?? throw new NotFoundException("Parent");

        var children = await _uow.Children.Query()
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

        var childIds = children.Select(c => c.Id).ToList();

        var publishedLessonsCount = await _uow.Lessons.Query()
            .CountAsync(l => l.IsPublished && !l.IsDeleted, ct);

        // [DESIGN FIX #2] Thay vì load toàn bộ LessonProgress vào RAM rồi tính bằng LINQ in-memory,
        // tính aggregate trực tiếp trên DB để giảm lượng dữ liệu truyền từ DB sang app server.
        // Với nhiều trẻ học nhiều bài, cách cũ có thể load hàng nghìn bản ghi không cần thiết.
        var childProgressStats = childIds.Count == 0
            ? new List<(Guid ChildId, int CompletedLessons, int TotalStars, int TimeSpent)>()
            : await _uow.LessonProgresses.Query()
                .Where(p => childIds.Contains(p.ChildId))
                .GroupBy(p => p.ChildId)
                .Select(g => new
                {
                    ChildId = g.Key,
                    CompletedLessons = g.Count(p => p.IsCompleted),
                    TimeSpent = g.Sum(p => p.TimeSpentSeconds)
                })
                .ToListAsync(ct)
                .ContinueWith(t => t.Result
                    .Select(x => (x.ChildId, x.CompletedLessons, 0, x.TimeSpent))
                    .ToList(), ct);

        var progressLookup = childProgressStats.ToDictionary(x => x.ChildId);

        var totalCompletedLessons = childProgressStats.Sum(x => x.CompletedLessons);
        var totalTimeSpent = childProgressStats.Sum(x => x.TimeSpent);
        var totalStars = children.Sum(c => c.TotalStars);

        var childItems = children.Select(child =>
        {
            progressLookup.TryGetValue(child.Id, out var stats);
            var completedLessons = stats.CompletedLessons;
            var timeSpentSeconds = stats.TimeSpent;
            var completionPercent = publishedLessonsCount == 0
                ? 0
                : (int)Math.Round((double)completedLessons / publishedLessonsCount * 100);

            return new ParentDashboardChildItemResponse(
                ChildId: child.Id,
                ChildName: child.Name,
                Age: child.Age,
                AvatarUrl: child.AvatarUrl,
                CompletedLessons: completedLessons,
                TotalStars: child.TotalStars,
                CurrentStreakDays: child.CurrentStreakDays,
                TimeSpentSeconds: timeSpentSeconds,
                CompletionPercent: completionPercent,
                LastLessonAt: child.LastLessonAt
            );
        }).ToList();

        // Load progresses for weekly chart (chỉ lấy 3 cột cần thiết thay vì toàn bộ entity)
        var childProgresses = childIds.Count == 0
            ? new List<KIDIO.Data.Entities.LessonProgress>()
            : await _uow.LessonProgresses.Query()
                .Where(p => childIds.Contains(p.ChildId))
                .Select(p => new KIDIO.Data.Entities.LessonProgress
                {
                    ChildId = p.ChildId,
                    IsCompleted = p.IsCompleted,
                    TimeSpentSeconds = p.TimeSpentSeconds,
                    UpdatedAt = p.UpdatedAt,
                    CompletedAt = p.CompletedAt,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync(ct);

        var weeklyProgress = BuildWeeklyProgress(childProgresses, weeks);

        var comparisons = childItems
            .OrderByDescending(x => x.CompletedLessons)
            .ThenByDescending(x => x.TotalStars)
            .ThenByDescending(x => x.TimeSpentSeconds)
            .Select((item, index) => new ChildComparisonResponse(
                ChildId: item.ChildId,
                ChildName: item.ChildName,
                CompletedLessons: item.CompletedLessons,
                TotalStars: item.TotalStars,
                TimeSpentSeconds: item.TimeSpentSeconds,
                Rank: index + 1))
            .ToList();

        return new ParentDashboardOverviewResponse(
            ParentId: parent.Id,
            ParentName: parent.DisplayName,
            TotalChildren: children.Count,
            TotalPublishedLessons: publishedLessonsCount,
            TotalLessonsCompleted: totalCompletedLessons,
            TotalStars: totalStars,
            TotalTimeSpentSeconds: totalTimeSpent,
            GeneratedAt: DateTime.UtcNow,
            Children: childItems,
            WeeklyProgress: weeklyProgress,
            Comparisons: comparisons
        );
    }

    private static List<WeeklyProgressResponse> BuildWeeklyProgress(
        List<KIDIO.Data.Entities.LessonProgress> childProgresses,
        int weeks)
    {
        var today = DateTime.UtcNow.Date;
        var startWeek = StartOfWeek(today).AddDays(-(weeks - 1) * 7);

        var result = new List<WeeklyProgressResponse>();

        for (var i = 0; i < weeks; i++)
        {
            var weekStart = startWeek.AddDays(i * 7);
            var weekEnd = weekStart.AddDays(6);

            var weekProgresses = childProgresses.Where(p =>
            {
                var activityDate = (p.UpdatedAt ?? p.CompletedAt ?? p.CreatedAt).Date;
                return activityDate >= weekStart && activityDate <= weekEnd;
            }).ToList();

            result.Add(new WeeklyProgressResponse(
                WeekStart: weekStart,
                WeekEnd: weekEnd,
                CompletedLessons: weekProgresses.Count(p => p.IsCompleted),
                TimeSpentSeconds: weekProgresses.Sum(p => p.TimeSpentSeconds),
                ActiveChildrenCount: weekProgresses.Select(p => p.ChildId).Distinct().Count()));
        }

        return result;
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return date.AddDays(-diff).Date;
    }
}
