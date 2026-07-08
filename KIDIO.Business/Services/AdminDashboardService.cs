using KIDIO.Business.DTOs.Dashboard;
using KIDIO.Business.Interfaces;
using KIDIO.Common.Enums;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUnitOfWork _uow;

    public AdminDashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AdminDashboardOverviewResponse> GetOverviewAsync(CancellationToken ct = default)
    {
        var totalParents = await _uow.Users.Query()
            .CountAsync(u => u.Role == UserRole.Parent, ct);

        var totalChildren = await _uow.Children.Query()
            .CountAsync(ct);

        var totalTopics = await _uow.Topics.Query()
            .CountAsync(t => t.IsActive, ct);

        var totalLessons = await _uow.Lessons.Query()
            .CountAsync(ct);

        var totalPublishedLessons = await _uow.Lessons.Query()
            .CountAsync(l => l.IsPublished, ct);

        var totalUnpublishedLessons = totalLessons - totalPublishedLessons;

        var totalLessonCompletions = await _uow.LessonProgresses.Query()
            .CountAsync(p => p.IsCompleted, ct);

        var totalVocabularies = await _uow.Vocabularies.Query()
            .CountAsync(ct);

        var totalAchievementsEarned = await _uow.Achievements.Query()
            .CountAsync(ct);

        return new AdminDashboardOverviewResponse(
            TotalParents: totalParents,
            TotalChildren: totalChildren,
            TotalTopics: totalTopics,
            TotalLessons: totalLessons,
            TotalPublishedLessons: totalPublishedLessons,
            TotalUnpublishedLessons: totalUnpublishedLessons,
            TotalLessonCompletions: totalLessonCompletions,
            TotalVocabularies: totalVocabularies,
            TotalAchievementsEarned: totalAchievementsEarned,
            GeneratedAt: DateTime.UtcNow
        );
    }

    public async Task<AdminDashboardDetailResponse> GetDetailAsync(
        int recentUsersCount = 10,
        int topLessonsCount = 10,
        int recentActivitiesCount = 10,
        CancellationToken ct = default)
    {
        var overview = await GetOverviewAsync(ct);

        // Recent registered users (Parents)
        var recentUsers = await _uow.Users.Query()
            .OrderByDescending(u => u.CreatedAt)
            .Take(recentUsersCount)
            .Select(u => new AdminRecentUserResponse(
                u.Id,
                u.DisplayName,
                u.Email,
                u.Role.ToString(),
                u.CreatedAt
            ))
            .ToListAsync(ct);

        // Top lessons by completion count
        var topLessons = await _uow.LessonProgresses.Query()
            .Where(p => p.IsCompleted)
            .GroupBy(p => p.LessonId)
            .Select(g => new
            {
                LessonId = g.Key,
                CompletionCount = g.Count(),
                AvgScorePercent = g.Average(p => (double)p.ScorePercent)
            })
            .OrderByDescending(x => x.CompletionCount)
            .Take(topLessonsCount)
            .ToListAsync(ct);

        var lessonIds = topLessons.Select(x => x.LessonId).ToList();
        var lessons = await _uow.Lessons.Query()
            .Include(l => l.Topic)
            .Where(l => lessonIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, ct);

        var topLessonResponses = topLessons.Select(x =>
        {
            lessons.TryGetValue(x.LessonId, out var lesson);
            return new AdminTopLessonResponse(
                LessonId: x.LessonId,
                Title: lesson?.Title ?? "Unknown",
                TopicName: lesson?.Topic?.Name ?? "Unknown",
                CompletionCount: x.CompletionCount,
                AvgScorePercent: Math.Round(x.AvgScorePercent, 1)
            );
        }).ToList();

        // 1. Recent Lessons (Started or Completed)
        var recentLessons = await _uow.LessonProgresses.Query()
            .Include(p => p.Child)
            .Include(p => p.Lesson)
            .OrderByDescending(p => p.CompletedAt ?? p.UpdatedAt ?? p.CreatedAt)
            .Take(recentActivitiesCount)
            .Select(p => new AdminRecentActivityResponse(
                p.ChildId,
                p.Child.Name,
                p.IsCompleted ? ActivityType.LessonCompleted.ToString() : ActivityType.LessonStarted.ToString(),
                p.IsCompleted ? $"Completed {p.Lesson.Title}" : $"Started {p.Lesson.Title}",
                p.IsCompleted ? p.StarsEarned.ToString() : null,
                p.CompletedAt ?? p.UpdatedAt ?? p.CreatedAt
            ))
            .ToListAsync(ct);

        // 2. Recent Pronunciation Scores
        var recentPronunciations = await _uow.PronunciationLogs.Query()
            .Include(p => p.Child)
            .OrderByDescending(p => p.CreatedAt)
            .Take(recentActivitiesCount)
            .Select(p => new AdminRecentActivityResponse(
                p.ChildId,
                p.Child.Name,
                ActivityType.PronunciationScored.ToString(),
                $"Pronunciation score {p.AccuracyScore}%",
                $"{p.AccuracyScore}%",
                p.CreatedAt
            ))
            .ToListAsync(ct);

        // 3. Recent Achievements
        var recentAchievements = await _uow.Achievements.Query()
            .Include(a => a.Child)
            .Include(a => a.AchievementDefinition)
            .OrderByDescending(a => a.EarnedAt)
            .Take(recentActivitiesCount)
            .Select(a => new AdminRecentActivityResponse(
                a.ChildId,
                a.Child.Name,
                ActivityType.AchievementEarned.ToString(),
                $"Earned {a.AchievementDefinition.Name}",
                null,
                a.EarnedAt
            ))
            .ToListAsync(ct);

        // Merge, sort, and take top N
        var recentActivities = recentLessons
            .Concat(recentPronunciations)
            .Concat(recentAchievements)
            .OrderByDescending(a => a.Timestamp)
            .Take(recentActivitiesCount)
            .ToList();

        // Chart: UserGrowth
        var userGrowthData = await _uow.Users.Query()
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);
        
        var userGrowth = userGrowthData.Select(x => new ChartPoint($"{x.Month:D2}/{x.Year}", x.Count)).ToList();

        // Chart: RevenueTrend
        var revenueTrendData = await _uow.PaymentTransactions.Query()
            .Where(p => p.Status == PaymentStatus.Success)
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(p => p.Amount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        var revenueTrend = revenueTrendData.Select(x => new ChartPoint($"{x.Month:D2}/{x.Year}", (int)x.Revenue)).ToList();

        // Chart: AgeDistribution
        var childrenAges = await _uow.Children.Query()
            .Select(c => c.Age)
            .ToListAsync(ct);

        var ageGroups = childrenAges.Select(age =>
        {
            if (age < 3) return "< 3";
            if (age <= 5) return "3-5";
            if (age <= 8) return "6-8";
            return "> 8";
        }).GroupBy(a => a)
          .Select(g => new ChartPoint(g.Key, g.Count()))
          .OrderBy(x => x.Label)
          .ToList();

        // Chart: ActivityTrend (Last 7 days combining LessonProgress + PronunciationLog)
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;
        
        var lessonCounts = await _uow.LessonProgresses.Query()
            .Where(p => p.CreatedAt >= sevenDaysAgo)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);
            
        var pronunciationCounts = await _uow.PronunciationLogs.Query()
            .Where(p => p.CreatedAt >= sevenDaysAgo)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var activityTrendDict = new Dictionary<DateTime, int>();
        for (int i = 0; i <= 7; i++) activityTrendDict[sevenDaysAgo.AddDays(i)] = 0;
        
        foreach(var lc in lessonCounts) if (activityTrendDict.ContainsKey(lc.Date)) activityTrendDict[lc.Date] += lc.Count;
        foreach(var pc in pronunciationCounts) if (activityTrendDict.ContainsKey(pc.Date)) activityTrendDict[pc.Date] += pc.Count;

        var activityTrend = activityTrendDict
            .OrderBy(x => x.Key)
            .Select(x => new ChartPoint(x.Key.ToString("dd/MM"), x.Value))
            .ToList();

        return new AdminDashboardDetailResponse(
            Overview: overview,
            RecentUsers: recentUsers,
            TopLessons: topLessonResponses,
            RecentActivities: recentActivities,
            UserGrowth: userGrowth,
            RevenueTrend: revenueTrend,
            AgeDistribution: ageGroups,
            ActivityTrend: activityTrend
        );
    }
}
