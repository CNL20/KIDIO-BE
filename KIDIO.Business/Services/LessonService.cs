using KIDIO.Business.DTOs.Lesson;
using KIDIO.Business.Extensions;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Common.Enums;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class LessonService : ILessonService
{
    private readonly IUnitOfWork _uow;

    public LessonService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<LessonSummaryResponse>> GetAllLessonsAsync(
        bool includeUnpublished = false, CancellationToken ct = default)
    {
        var query = _uow.Lessons.Query();

        if (!includeUnpublished)
            query = query.Where(l => l.IsPublished);

        return await query
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonSummaryResponse(
                l.Id,
                l.Title,
                l.Type.ToString(),
                l.Difficulty.ToString(),
                l.SkillFocus.ToString(),
                l.DurationSeconds,
                l.ThumbnailUrl,
                l.OrderIndex,
                l.IsPublished
            ))
            .ToListAsync(ct);
    }

    public async Task<PagedResponse<LessonSummaryResponse>> GetAllLessonsPagedAsync(
        bool includeUnpublished = false, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = _uow.Lessons.Query();

        if (!includeUnpublished)
            query = query.Where(l => l.IsPublished);

        return await query
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonSummaryResponse(
                l.Id,
                l.Title,
                l.Type.ToString(),
                l.Difficulty.ToString(),
                l.SkillFocus.ToString(),
                l.DurationSeconds,
                l.ThumbnailUrl,
                l.OrderIndex,
                l.IsPublished
            ))
            .ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<List<LessonSummaryResponse>> GetLessonsByTopicAsync(
        Guid topicId, bool includeUnpublished = false, CancellationToken ct = default)
    {
        _ = await _uow.Topics.GetByIdAsync(topicId, ct)
            ?? throw new NotFoundException("Topic");

        var query = _uow.Lessons.Query()
            .Where(l => l.TopicId == topicId);

        if (!includeUnpublished)
            query = query.Where(l => l.IsPublished);

        return await query
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonSummaryResponse(
                l.Id,
                l.Title,
                l.Type.ToString(),
                l.Difficulty.ToString(),
                l.SkillFocus.ToString(),
                l.DurationSeconds,
                l.ThumbnailUrl,
                l.OrderIndex,
                l.IsPublished
            ))
            .ToListAsync(ct);
    }

    public async Task<PagedResponse<LessonSummaryResponse>> GetLessonsByTopicPagedAsync(
        Guid topicId, bool includeUnpublished = false, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        _ = await _uow.Topics.GetByIdAsync(topicId, ct)
            ?? throw new NotFoundException("Topic");

        var query = _uow.Lessons.Query()
            .Where(l => l.TopicId == topicId);

        if (!includeUnpublished)
            query = query.Where(l => l.IsPublished);

        return await query
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonSummaryResponse(
                l.Id,
                l.Title,
                l.Type.ToString(),
                l.Difficulty.ToString(),
                l.SkillFocus.ToString(),
                l.DurationSeconds,
                l.ThumbnailUrl,
                l.OrderIndex,
                l.IsPublished
            ))
            .ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<LessonResponse> GetLessonByIdAsync(
        Guid lessonId, bool includeUnpublished = false, CancellationToken ct = default)
    {
        var lesson = await _uow.Lessons.Query()
            .Include(l => l.Topic)
            .Include(l => l.Vocabularies)
            .FirstOrDefaultAsync(l => l.Id == lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        if (!includeUnpublished && !lesson.IsPublished)
            throw new NotFoundException("Lesson");

        return MapToResponse(lesson);
    }

    public async Task<LessonResponse> CreateLessonAsync(
        CreateLessonRequest request, CancellationToken ct = default)
    {
        _ = await _uow.Topics.GetByIdAsync(request.TopicId, ct)
            ?? throw new NotFoundException("Topic");

        // [FIX #8] Dùng IgnoreQueryFilters() và chỉ bắt lỗi nếu bản ghi đó chưa bị xóa.
        // Trước đó, global filter ẩn soft-deleted records, nhưng do AnyAsync()
        // chỉ thấy active records, admin vẫn không tạo được lesson trùng tên dù đã xóa bản ghi cũ.
        var existingLesson = await _uow.Lessons.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.TopicId == request.TopicId &&
                                      l.Title.ToLower() == request.Title.ToLower().Trim(), ct);

        if (existingLesson != null && !existingLesson.IsDeleted)
            throw new AppException($"Lesson title '{request.Title}' already exists in this topic.");

        var lesson = new Lesson
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Type = ParseEnum<LessonType>(request.LessonType),
            Difficulty = ParseEnum<DifficultyLevel>(request.Difficulty),
            SkillFocus = ParseEnum<SkillType>(request.SkillFocus),
            DurationSeconds = request.DurationSeconds,
            ThumbnailUrl = request.ThumbnailUrl,
            AudioUrl = request.AudioUrl,
            VideoUrl = request.VideoUrl,
            ContentJson = request.ContentJson,
            OrderIndex = request.OrderIndex,
            TopicId = request.TopicId,
            IsPublished = false  // mặc định draft
        };

        await _uow.Lessons.AddAsync(lesson, ct);
        await _uow.SaveChangesAsync(ct);

        // Load lại để có Topic name
        return await GetLessonByIdAsync(lesson.Id, includeUnpublished: true, ct);
    }

    public async Task<LessonResponse> UpdateLessonAsync(
        Guid lessonId, UpdateLessonRequest request, CancellationToken ct = default)
    {
        var lesson = await _uow.Lessons.GetByIdAsync(lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        // [FIX #8] Dùng IgnoreQueryFilters() và chỉ bảo vệ khi bản ghi cùng tên chưa bị xóa.
        var existingLesson = await _uow.Lessons.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.TopicId == lesson.TopicId &&
                                      l.Id != lessonId &&
                                      l.Title.ToLower() == request.Title.ToLower().Trim(), ct);

        if (existingLesson != null && !existingLesson.IsDeleted)
            throw new AppException($"Lesson title '{request.Title}' already exists in this topic.");

        lesson.Title = request.Title.Trim();
        lesson.Description = request.Description;
        lesson.Type = ParseEnum<LessonType>(request.LessonType);
        lesson.Difficulty = ParseEnum<DifficultyLevel>(request.Difficulty);
        lesson.SkillFocus = ParseEnum<SkillType>(request.SkillFocus);
        lesson.DurationSeconds = request.DurationSeconds;
        lesson.ThumbnailUrl = request.ThumbnailUrl;
        lesson.AudioUrl = request.AudioUrl;
        lesson.VideoUrl = request.VideoUrl;
        lesson.ContentJson = request.ContentJson;
        lesson.OrderIndex = request.OrderIndex;
        lesson.IsPublished = request.IsPublished;

        _uow.Lessons.Update(lesson);
        await _uow.SaveChangesAsync(ct);

        return await GetLessonByIdAsync(lesson.Id, includeUnpublished: true, ct);
    }

    public async Task DeleteLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        var lesson = await _uow.Lessons.GetByIdAsync(lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        if (lesson.IsPublished)
            throw new AppException("Unpublish the lesson before deleting it.");

        lesson.IsDeleted = true;
        _uow.Lessons.Update(lesson);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task PublishLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        var lesson = await _uow.Lessons.GetByIdAsync(lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        if (lesson.IsPublished)
            throw new AppException("Lesson is already published.");

        lesson.IsPublished = true;
        _uow.Lessons.Update(lesson);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task UnpublishLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        var lesson = await _uow.Lessons.GetByIdAsync(lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        if (!lesson.IsPublished)
            throw new AppException("Lesson is not published.");

        lesson.IsPublished = false;
        _uow.Lessons.Update(lesson);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RestoreLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        var lesson = await _uow.Lessons.Query()
            .IgnoreQueryFilters()
            .Include(l => l.Vocabularies)
            .FirstOrDefaultAsync(l => l.Id == lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        if (!lesson.IsDeleted)
            throw new AppException("Lesson is not deleted.");

        lesson.IsDeleted = false;
        _uow.Lessons.Update(lesson);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task HardDeleteLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        var lesson = await _uow.Lessons.Query()
            .IgnoreQueryFilters()
            .Include(l => l.Vocabularies)
            .FirstOrDefaultAsync(l => l.Id == lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        if (lesson.IsPublished)
            throw new AppException("Cannot permanently delete a published lesson. Unpublish first.");

        // Remove vocabularies explicitly (or rely on cascade) then remove lesson
        _uow.Lessons.Remove(lesson);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Helpers ─────────────────────────────────────────────

    private static T ParseEnum<T>(string value) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(value, ignoreCase: true, out var result))
            throw new AppException($"Invalid value '{value}' for {typeof(T).Name}.");
        return result;
    }

    private static LessonResponse MapToResponse(Lesson l) => new(
        Id: l.Id,
        Title: l.Title,
        Description: l.Description,
        LessonType: l.Type.ToString(),
        Difficulty: l.Difficulty.ToString(),
        SkillFocus: l.SkillFocus.ToString(),
        DurationSeconds: l.DurationSeconds,
        ThumbnailUrl: l.ThumbnailUrl,
        AudioUrl: l.AudioUrl,
        VideoUrl: l.VideoUrl,
        ContentJson: l.ContentJson,
        OrderIndex: l.OrderIndex,
        IsPublished: l.IsPublished,
        TopicId: l.TopicId,
        TopicName: l.Topic?.Name ?? string.Empty,
        TotalVocabularies: l.Vocabularies?.Count(v => !v.IsDeleted) ?? 0,
        CreatedAt: l.CreatedAt
    );
}