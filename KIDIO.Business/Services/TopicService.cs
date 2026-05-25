using KIDIO.Business.DTOs.Lesson;
using KIDIO.Business.Extensions;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
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

    public async Task<List<TopicSummaryResponse>> GetAllTopicsAsync(CancellationToken ct = default)
    {
        var topics = await _uow.Topics.Query()
            .Where(t => t.IsActive)
            .OrderBy(t => t.OrderIndex)
            .Select(t => new TopicSummaryResponse(
                t.Id,
                t.Name,
                t.IconUrl,
                t.OrderIndex,
                t.Lessons.Count(l => l.IsPublished && !l.IsDeleted)
            ))
            .ToListAsync(ct);

        return topics;
    }

    public async Task<PagedResponse<TopicSummaryResponse>> GetTopicsPagedAsync(
        int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = _uow.Topics.Query()
            .Where(t => t.IsActive)
            .OrderBy(t => t.OrderIndex)
            .Select(t => new TopicSummaryResponse(
                t.Id,
                t.Name,
                t.IconUrl,
                t.OrderIndex,
                t.Lessons.Count(l => l.IsPublished && !l.IsDeleted)
            ));

        return await query.ToPagedResponseAsync(pageNumber, pageSize, ct);
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
        // Kiểm tra tên trùng
        var exists = await _uow.Topics.FirstOrDefaultAsync(
            t => t.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists is not null)
            throw new AppException("A topic with this name already exists.");

        // Kiểm tra OrderIndex trùng
        var orderIndexExists = await _uow.Topics.FirstOrDefaultAsync(
            t => t.OrderIndex == request.OrderIndex, ct);

        if (orderIndexExists is not null)
            throw new AppException("A topic with this OrderIndex already exists.");

        var topic = new Topic
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            IconUrl = request.IconUrl,
            OrderIndex = request.OrderIndex,
            IsActive = true
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

        // Kiểm tra tên trùng với topic khác
        var duplicate = await _uow.Topics.FirstOrDefaultAsync(
            t => t.Name.ToLower() == request.Name.ToLower() && t.Id != topicId, ct);

        if (duplicate is not null)
            throw new AppException("A topic with this name already exists.");

        // Kiểm tra OrderIndex trùng với topic khác
        var orderIndexDuplicate = await _uow.Topics.FirstOrDefaultAsync(
            t => t.OrderIndex == request.OrderIndex && t.Id != topicId, ct);

        if (orderIndexDuplicate is not null)
            throw new AppException("A topic with this OrderIndex already exists.");

        topic.Name = request.Name.Trim();
        topic.Description = request.Description;
        topic.IconUrl = request.IconUrl;
        topic.OrderIndex = request.OrderIndex;
        topic.IsActive = request.IsActive;

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

        // Không cho xóa nếu còn lesson đã published
        var hasPublished = topic.Lessons.Any(l => l.IsPublished && !l.IsDeleted);
        if (hasPublished)
            throw new AppException(
                "Cannot delete a topic that has published lessons. Unpublish all lessons first.");

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

        // Không cho xóa vĩnh viễn nếu còn lesson đã published
        var hasPublished = topic.Lessons.Any(l => l.IsPublished && !l.IsDeleted);
        if (hasPublished)
            throw new AppException(
                "Cannot permanently delete a topic that has published lessons. Unpublish or remove lessons first.");

        _uow.Topics.Remove(topic);
        await _uow.SaveChangesAsync(ct);
    }

    private static TopicResponse MapToResponse(Topic t) => new(
        Id: t.Id,
        Name: t.Name,
        Description: t.Description,
        IconUrl: t.IconUrl,
        OrderIndex: t.OrderIndex,
        IsActive: t.IsActive,
        TotalLessons: t.Lessons?.Count(l => l.IsPublished && !l.IsDeleted) ?? 0,
        CreatedAt: t.CreatedAt
    );
}