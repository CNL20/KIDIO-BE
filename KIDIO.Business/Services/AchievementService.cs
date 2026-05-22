using KIDIO.Business.DTOs.Achievement;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class AchievementService : IAchievementService
{
    private readonly IUnitOfWork _uow;

    public AchievementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<AchievementResponse>> GetByChildAsync(
        Guid childId, Guid parentId, CancellationToken ct = default)
    {
        var child = await _uow.Children.GetByIdAsync(childId, ct)
            ?? throw new NotFoundException("Child");

        if (child.ParentId != parentId)
            throw new ForbiddenException("You do not have access to this child profile.");

        return await _uow.Achievements.Query()
            .Where(a => a.ChildId == childId)
            .OrderByDescending(a => a.EarnedAt)
            .Select(a => new AchievementResponse(
                a.Id,
                a.Name,
                a.Description,
                a.BadgeUrl,
                a.AchievementType,
                a.Threshold,
                a.EarnedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<AchievementUnlockResult> CheckAndUnlockAsync(
        Guid childId, CancellationToken ct = default)
    {
        var child = await _uow.Children.GetByIdAsync(childId, ct)
            ?? throw new NotFoundException("Child");

        // Lấy achievements child đã có
        var existing = await _uow.Achievements.Query()
            .Where(a => a.ChildId == childId)
            .Select(a => new { a.AchievementType, a.Threshold })
            .ToListAsync(ct);

        var existingSet = existing
            .Select(a => $"{a.AchievementType}:{a.Threshold}")
            .ToHashSet();

        // Lấy tất cả achievement definition từ DB (thay vì hardcode)
        var definitions = await _uow.AchievementDefinitions.Query()
            .Where(d => d.IsActive)
            .OrderBy(d => d.OrderIndex)
            .ToListAsync(ct);

        // Lấy số lesson đã hoàn thành
        var lessonsCompleted = await _uow.LessonProgresses.Query()
            .CountAsync(p => p.ChildId == childId && p.IsCompleted, ct);

        // Kiểm tra từng milestone trong định nghĩa
        var newAchievements = new List<Achievement>();

        foreach (var def in definitions)
        {
            var key = $"{def.Type}:{def.Threshold}";

            // Bỏ qua nếu đã có
            if (existingSet.Contains(key)) continue;

            var unlocked = def.Type switch
            {
                "Stars" => child.TotalStars >= def.Threshold,
                "Streak" => child.CurrentStreakDays >= def.Threshold,
                "Lessons" => lessonsCompleted >= def.Threshold,
                _ => false
            };

            if (!unlocked) continue;

            var achievement = new Achievement
            {
                ChildId = childId,
                Name = def.Name,
                Description = def.Description,
                BadgeUrl = def.BadgeUrl,
                AchievementType = def.Type,
                Threshold = def.Threshold,
                EarnedAt = DateTime.UtcNow
            };

            newAchievements.Add(achievement);
            await _uow.Achievements.AddAsync(achievement, ct);
        }

        if (newAchievements.Any())
            await _uow.SaveChangesAsync(ct);

        var responses = newAchievements
            .Select(a => new AchievementResponse(
                a.Id,
                a.Name,
                a.Description,
                a.BadgeUrl,
                a.AchievementType,
                a.Threshold,
                a.EarnedAt
            ))
            .ToList();

        return new AchievementUnlockResult(
            NewAchievements: responses,
            HasNew: responses.Any()
        );
    }

    // =============== Admin methods for managing achievement definitions ===============

    public async Task<List<AchievementDefinitionResponse>> GetAllDefinitionsAsync(CancellationToken ct = default)
    {
        return await _uow.AchievementDefinitions.Query()
            .OrderBy(d => d.OrderIndex)
            .Select(d => new AchievementDefinitionResponse(
                d.Id,
                d.Type,
                d.Threshold,
                d.Name,
                d.Description,
                d.BadgeUrl,
                d.OrderIndex,
                d.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<AchievementDefinitionResponse?> GetDefinitionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var definition = await _uow.AchievementDefinitions.GetByIdAsync(id, ct);
        if (definition == null) return null;

        return new AchievementDefinitionResponse(
            definition.Id,
            definition.Type,
            definition.Threshold,
            definition.Name,
            definition.Description,
            definition.BadgeUrl,
            definition.OrderIndex,
            definition.IsActive
        );
    }

    public async Task<AchievementDefinitionResponse> CreateDefinitionAsync(
        CreateAchievementDefinitionRequest request, CancellationToken ct = default)
    {
        // Check if definition with same type and threshold already exists
        var existing = await _uow.AchievementDefinitions.Query()
            .FirstOrDefaultAsync(d => d.Type == request.Type && d.Threshold == request.Threshold, ct);

        if (existing != null && !existing.IsDeleted)
            throw new AppException($"Achievement definition with type '{request.Type}' and threshold {request.Threshold} already exists.");

        // Get next OrderIndex
        var maxOrder = await _uow.AchievementDefinitions.Query()
            .MaxAsync(d => (int?)d.OrderIndex, ct) ?? 0;

        var definition = new AchievementDefinition
        {
            Type = request.Type,
            Threshold = request.Threshold,
            Name = request.Name,
            Description = request.Description,
            BadgeUrl = request.BadgeUrl,
            OrderIndex = maxOrder + 1,
            IsActive = true
        };

        await _uow.AchievementDefinitions.AddAsync(definition, ct);
        await _uow.SaveChangesAsync(ct);

        return new AchievementDefinitionResponse(
            definition.Id,
            definition.Type,
            definition.Threshold,
            definition.Name,
            definition.Description,
            definition.BadgeUrl,
            definition.OrderIndex,
            definition.IsActive
        );
    }

    public async Task<AchievementDefinitionResponse?> UpdateDefinitionAsync(
        Guid id, UpdateAchievementDefinitionRequest request, CancellationToken ct = default)
    {
        var definition = await _uow.AchievementDefinitions.GetByIdAsync(id, ct);
        if (definition == null) return null;

        // Check if another definition with same type and threshold exists
        var existing = await _uow.AchievementDefinitions.Query()
            .FirstOrDefaultAsync(d => d.Id != id && d.Type == request.Type && d.Threshold == request.Threshold, ct);

        if (existing != null && !existing.IsDeleted)
            throw new AppException($"Achievement definition with type '{request.Type}' and threshold {request.Threshold} already exists.");

        definition.Type = request.Type;
        definition.Threshold = request.Threshold;
        definition.Name = request.Name;
        definition.Description = request.Description;
        definition.BadgeUrl = request.BadgeUrl;
        definition.OrderIndex = request.OrderIndex;
        definition.IsActive = request.IsActive;

        _uow.AchievementDefinitions.Update(definition);
        await _uow.SaveChangesAsync(ct);

        return new AchievementDefinitionResponse(
            definition.Id,
            definition.Type,
            definition.Threshold,
            definition.Name,
            definition.Description,
            definition.BadgeUrl,
            definition.OrderIndex,
            definition.IsActive
        );
    }

    public async Task<bool> DeleteDefinitionAsync(Guid id, CancellationToken ct = default)
    {
        var definition = await _uow.AchievementDefinitions.GetByIdAsync(id, ct);
        if (definition == null) return false;

        // Soft delete
        _uow.AchievementDefinitions.Remove(definition);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RestoreDefinitionAsync(Guid id, CancellationToken ct = default)
    {
        var definition = await _uow.AchievementDefinitions.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (definition == null || !definition.IsDeleted)
            return false;

        definition.IsDeleted = false;
        _uow.AchievementDefinitions.Update(definition);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> HardDeleteDefinitionAsync(Guid id, CancellationToken ct = default)
    {
        var definition = await _uow.AchievementDefinitions.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (definition == null)
            return false;

        // Kiểm tra xem có achievement nào reference definition này không
        var referencedAchievements = await _uow.Achievements.Query()
            .AnyAsync(a => a.AchievementType == definition.Type && a.Threshold == definition.Threshold, ct);

        if (referencedAchievements)
            throw new AppException("Cannot hard delete achievement definition that has been earned by children.");

        // Hard delete
        _uow.AchievementDefinitions.Query()
            .IgnoreQueryFilters()
            .Where(d => d.Id == id)
            .ExecuteDelete();
        
        return true;
    }
}