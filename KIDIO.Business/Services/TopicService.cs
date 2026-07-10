using KIDIO.Business.DTOs.Lesson;
using KIDIO.Business.Extensions;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Common.Enums;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class TopicService : ITopicService
{
    private readonly IUnitOfWork _uow;

    public TopicService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // Lấy tất cả Topic cho Admin (không cần childId - IsUnlocked luôn true)
    public async Task<List<TopicSummaryResponse>> GetAllTopicsAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _uow.Topics.Query();
        
        if (!includeInactive)
            query = query.Where(t => t.IsActive);

        return await query
            .OrderBy(t => t.LevelNumber).ThenBy(t => t.OrderIndex)
            .Select(t => new TopicSummaryResponse(
                t.Id,
                t.Name,
                t.IconUrl,
                t.OrderIndex,
                t.Lessons.Count(l => l.IsPublished && !l.IsDeleted),
                t.IsActive,
                t.CreatedAt,
                t.Access.ToString(),
                t.LevelNumber,
                true // Admin luôn thấy tất cả là unlocked
            ))
            .ToListAsync(ct);
    }

    public async Task<List<TopicSummaryResponse>> GetAllTopicsForChildAsync(
        Guid childId, CancellationToken ct = default)
    {
        var child = await _uow.Children.GetByIdAsync(childId, ct)
            ?? throw new NotFoundException("Child");

        var topics = await _uow.Topics.Query()
            .Where(t => t.IsActive)
            .OrderBy(t => t.LevelNumber).ThenBy(t => t.OrderIndex)
            .Select(t => new
            {
                t.Id, t.Name, t.IconUrl, t.OrderIndex, t.IsActive, t.CreatedAt,
                t.Access, t.LevelNumber,
                TotalLessons = t.Lessons.Count(l => l.IsPublished && !l.IsDeleted),
                LessonIds = t.Lessons.Where(l => l.IsPublished && !l.IsDeleted).Select(l => l.Id).ToList()
            })
            .ToListAsync(ct);

        var completedLessonList = await _uow.LessonProgresses.Query()
            .Where(p => p.ChildId == childId && p.IsCompleted)
            .Select(p => p.LessonId)
            .ToListAsync(ct);
        var completedLessonIds = completedLessonList.ToHashSet();

        int currentLevel = (int)child.StartingLevel;
        var groupedLevels = topics.GroupBy(t => t.LevelNumber).OrderBy(g => g.Key).ToList();
        
        foreach (var group in groupedLevels)
        {
            if (group.Key < currentLevel) continue;
            
            bool isLevelCompleted = true;
            foreach (var t in group)
            {
                bool isTopicCompleted = t.TotalLessons > 0 && t.LessonIds.All(id => completedLessonIds.Contains(id));
                if (!isTopicCompleted && t.TotalLessons > 0)
                {
                    isLevelCompleted = false;
                    break;
                }
            }
            
            if (isLevelCompleted)
            {
                currentLevel = group.Key + 1;
            }
            else
            {
                break;
            }
        }

        return topics.Select(t => new TopicSummaryResponse(
            t.Id,
            t.Name,
            t.IconUrl,
            t.OrderIndex,
            t.TotalLessons,
            t.IsActive,
            t.CreatedAt,
            t.Access.ToString(),
            t.LevelNumber,
            IsUnlocked: t.LevelNumber <= currentLevel
        )).ToList();
    }

    public async Task<PagedResponse<TopicSummaryResponse>> GetTopicsPagedAsync(
        int pageNumber = 1, int pageSize = 10, bool includeInactive = false, string? keyword = null, CancellationToken ct = default)
    {
        var query = _uow.Topics.Query();

        if (!includeInactive)
            query = query.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(kw) || 
                                    (t.Description != null && t.Description.ToLower().Contains(kw)));
        }

        var mappedQuery = query
            .OrderBy(t => t.LevelNumber).ThenBy(t => t.OrderIndex)
            .Select(t => new TopicSummaryResponse(
                t.Id,
                t.Name,
                t.IconUrl,
                t.OrderIndex,
                t.Lessons.Count(l => l.IsPublished && !l.IsDeleted),
                t.IsActive,
                t.CreatedAt,
                t.Access.ToString(),
                t.LevelNumber,
                true // Admin paged - luôn unlocked
            ));

        return await mappedQuery.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<PagedResponse<TopicSummaryResponse>> GetDeletedTopicsPagedAsync(
        int pageNumber = 1, int pageSize = 10, string? keyword = null, CancellationToken ct = default)
    {
        var query = _uow.Topics.Query()
            .IgnoreQueryFilters()
            .Where(t => t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(kw) || 
                                    (t.Description != null && t.Description.ToLower().Contains(kw)));
        }

        var mappedQuery = query
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .Select(t => new TopicSummaryResponse(
                t.Id,
                t.Name,
                t.IconUrl,
                t.OrderIndex,
                0, // không cần count lesson cho thùng rác
                t.IsActive,
                t.CreatedAt,
                t.Access.ToString(),
                t.LevelNumber,
                true
            ));

        return await mappedQuery.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<TopicResponse> GetTopicByIdAsync(Guid topicId, CancellationToken ct = default)
    {
        var topic = await _uow.Topics.Query()
            .Include(t => t.Lessons)
            .FirstOrDefaultAsync(t => t.Id == topicId, ct)
            ?? throw new NotFoundException("Topic");

        return MapToResponse(topic);
    }

    public async Task<TopicResponse> CreateTopicAsync(
        CreateTopicRequest request, CancellationToken ct = default)
    {
        // [FIX #7] Dùng IgnoreQueryFilters() để phát hiện cả soft-deleted records
        var exists = await _uow.Topics.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists is not null && !exists.IsDeleted)
            throw new AppException("A topic with this name already exists.");

        if (exists is not null && exists.IsDeleted)
            throw new AppException("A topic with this name was previously deleted. Please restore it or use a different name.");

        var targetLevel = request.LevelNumber ?? 1;
        var orderIndexExists = await _uow.Topics.FirstOrDefaultAsync(
            t => t.OrderIndex == request.OrderIndex && t.LevelNumber == targetLevel, ct);

        if (orderIndexExists is not null)
            throw new AppException($"A topic with OrderIndex {request.OrderIndex} already exists in Level {targetLevel}.");

        var topic = new Topic
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            IconUrl = request.IconUrl,
            OrderIndex = request.OrderIndex,
            IsActive = request.IsActive ?? true,
            Access = ParseEnum<AccessType>(request.Access ?? "Free"),
            LevelNumber = request.LevelNumber ?? 1
        };

        await _uow.Topics.AddAsync(topic, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToResponse(topic);
    }

    public async Task<TopicResponse> UpdateTopicAsync(
        Guid topicId, UpdateTopicRequest request, CancellationToken ct = default)
    {
        var topic = await _uow.Topics.Query()
            .Include(t => t.Lessons)
            .FirstOrDefaultAsync(t => t.Id == topicId, ct)
            ?? throw new NotFoundException("Topic");

        // [FIX #7] Dùng IgnoreQueryFilters() để phát hiện cả soft-deleted records
        var duplicate = await _uow.Topics.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower() && t.Id != topicId, ct);

        if (duplicate is not null && !duplicate.IsDeleted)
            throw new AppException("A topic with this name already exists.");

        if (duplicate is not null && duplicate.IsDeleted)
            throw new AppException("A previously deleted topic has this name. Please use a different name.");

        // Kiểm tra OrderIndex trùng với topic khác trong cùng Level
        var targetLevel = request.LevelNumber ?? 1;
        var orderIndexDuplicate = await _uow.Topics.FirstOrDefaultAsync(
            t => t.OrderIndex == request.OrderIndex && t.LevelNumber == targetLevel && t.Id != topicId, ct);

        if (orderIndexDuplicate is not null)
            throw new AppException($"A topic with OrderIndex {request.OrderIndex} already exists in Level {targetLevel}.");

        topic.Name = request.Name.Trim();
        topic.Description = request.Description;
        topic.IconUrl = request.IconUrl;
        topic.OrderIndex = request.OrderIndex;
        topic.IsActive = request.IsActive;
        topic.Access = ParseEnum<AccessType>(request.Access ?? "Free");
        topic.LevelNumber = request.LevelNumber ?? 1;

        _uow.Topics.Update(topic);
        await _uow.SaveChangesAsync(ct);

        return MapToResponse(topic);
    }

    public async Task DeleteTopicAsync(Guid topicId, CancellationToken ct = default)
    {
        var topic = await _uow.Topics.Query()
            .Include(t => t.Lessons)
            .FirstOrDefaultAsync(t => t.Id == topicId, ct)
            ?? throw new NotFoundException("Topic");

        var hasPublished = topic.Lessons.Any(l => l.IsPublished && !l.IsDeleted);
        if (hasPublished)
            throw new AppException("Cannot delete a topic that has published lessons. Unpublish all lessons first.");

        topic.IsDeleted = true;
        _uow.Topics.Update(topic);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RestoreTopicAsync(Guid topicId, CancellationToken ct = default)
    {
        var topic = await _uow.Topics.Query()
            .IgnoreQueryFilters()
            .Include(t => t.Lessons)
            .FirstOrDefaultAsync(t => t.Id == topicId, ct)
            ?? throw new NotFoundException("Topic");

        if (!topic.IsDeleted)
            throw new AppException("Topic is not deleted.");

        topic.IsDeleted = false;
        _uow.Topics.Update(topic);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task HardDeleteTopicAsync(Guid topicId, CancellationToken ct = default)
    {
        var topic = await _uow.Topics.Query()
            .IgnoreQueryFilters()
            .Include(t => t.Lessons)
            .FirstOrDefaultAsync(t => t.Id == topicId, ct)
            ?? throw new NotFoundException("Topic");

        var hasPublished = topic.Lessons.Any(l => l.IsPublished && !l.IsDeleted);
        if (hasPublished)
            throw new AppException("Cannot permanently delete a topic that has published lessons. Unpublish or remove lessons first.");

        _uow.Topics.Remove(topic);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Helpers ─────────────────────────────────────────────

    private static T ParseEnum<T>(string value) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(value, ignoreCase: true, out var result))
            throw new AppException($"Invalid value '{value}' for {typeof(T).Name}.");
        return result;
    }

    private static TopicResponse MapToResponse(Topic t) => new(
        Id: t.Id,
        Name: t.Name,
        Description: t.Description,
        IconUrl: t.IconUrl,
        OrderIndex: t.OrderIndex,
        IsActive: t.IsActive,
        TotalLessons: t.Lessons?.Count(l => l.IsPublished && !l.IsDeleted) ?? 0,
        CreatedAt: t.CreatedAt,
        Access: t.Access.ToString(),
        LevelNumber: t.LevelNumber
    );
}