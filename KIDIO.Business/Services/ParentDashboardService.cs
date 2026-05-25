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

        var childProgresses = childIds.Count == 0
            ? new List<KIDIO.Data.Entities.LessonProgress>()
            : await _uow.LessonProgresses.Query()
                .Where(p => childIds.Contains(p.ChildId))
                .ToListAsync(ct);

        var completedProgresses = childProgresses.Where(p => p.IsCompleted).ToList();
        var totalCompletedLessons = completedProgresses.Count;
        var totalTimeSpent = childProgresses.Sum(p => p.TimeSpentSeconds);
        var totalStars = children.Sum(c => c.TotalStars);

        var childProgressLookup = childProgresses
            .GroupBy(p => p.ChildId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var childItems = children.Select(child =>
        {
            childProgressLookup.TryGetValue(child.Id, out var progresses);
            progresses ??= new List<KIDIO.Data.Entities.LessonProgress>();

            var completedLessons = progresses.Count(p => p.IsCompleted);
            var timeSpentSeconds = progresses.Sum(p => p.TimeSpentSeconds);
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
