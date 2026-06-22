using KIDIO.Business.DTOs.Achievement;
using KIDIO.Business.Extensions;
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
            .Include(a => a.AchievementDefinition) // Include để lấy thông tin định nghĩa
            .Where(a => a.ChildId == childId)
            .OrderByDescending(a => a.EarnedAt)
            .Select(a => new AchievementResponse(
                a.Id,
                a.AchievementDefinition.Name,        // Lấy từ bảng Definition
                a.AchievementDefinition.Description, // Lấy từ bảng Definition
                a.AchievementDefinition.BadgeUrl,    // Lấy từ bảng Definition
                a.AchievementDefinition.Type,        // Lấy từ bảng Definition
                a.AchievementDefinition.Threshold,   // Lấy từ bảng Definition
                a.EarnedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<PagedResponse<AchievementResponse>> GetByChildPagedAsync(
        Guid childId, Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var child = await _uow.Children.GetByIdAsync(childId, ct)
            ?? throw new NotFoundException("Child");

        if (child.ParentId != parentId)
            throw new ForbiddenException("You do not have access to this child profile.");

        var query = _uow.Achievements.Query()
            .Include(a => a.AchievementDefinition) // Include để lấy thông tin định nghĩa
            .Where(a => a.ChildId == childId)
            .OrderByDescending(a => a.EarnedAt)
            .Select(a => new AchievementResponse(
                a.Id,
                a.AchievementDefinition.Name,        // Lấy từ bảng Definition
                a.AchievementDefinition.Description, // Lấy từ bảng Definition
                a.AchievementDefinition.BadgeUrl,    // Lấy từ bảng Definition
                a.AchievementDefinition.Type,        // Lấy từ bảng Definition
                a.AchievementDefinition.Threshold,   // Lấy từ bảng Definition
                a.EarnedAt
            ));

        return await query.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<AchievementUnlockResult> CheckAndUnlockAsync(
        Guid childId, CancellationToken ct = default)
    {
        var child = await _uow.Children.GetByIdAsync(childId, ct)
            ?? throw new NotFoundException("Child");

        // Lấy danh sách ID của các achievement định nghĩa mà child đã đạt được
        var existingSet = await _uow.Achievements.Query()
            .Where(a => a.ChildId == childId)
            .Select(a => a.AchievementDefinitionId)
            .ToListAsync(ct);

        // Lấy tất cả các định nghĩa đang kích hoạt
        var definitions = await _uow.AchievementDefinitions.Query()
            .Where(d => d.IsActive)
            .OrderBy(d => d.OrderIndex)
            .ToListAsync(ct);

        // Lấy số lesson đã hoàn thành
        var lessonsCompleted = await _uow.LessonProgresses.Query()
            .CountAsync(p => p.ChildId == childId && p.IsCompleted, ct);

        // Lấy số từ vựng bé đã phát âm đạt yêu cầu (Accuracy >= 60)
        var wordsLearned = await _uow.PronunciationLogs.Query()
            .Where(p => p.ChildId == childId && p.AccuracyScore >= 60)
            .Select(p => p.TargetText)
            .Distinct()
            .CountAsync(ct);

        var newAchievements = new List<Achievement>();

        foreach (var def in definitions)
        {
            // Kiểm tra trùng bằng ID thay vì chuỗi kết hợp
            if (existingSet.Contains(def.Id)) continue;

            var unlocked = def.Type switch
            {
                "Stars" => child.TotalStars >= def.Threshold,
                "Streak" => child.CurrentStreakDays >= def.Threshold,
                "Lessons" => lessonsCompleted >= def.Threshold,
                "Words" => wordsLearned >= def.Threshold,
                _ => false
            };

            if (!unlocked) continue;

            var achievement = new Achievement
            {
                ChildId = childId,
                AchievementDefinitionId = def.Id, // Lưu ID của Định nghĩa gốc liên kết tới
                EarnedAt = DateTime.UtcNow
            };

            newAchievements.Add(achievement);
            await _uow.Achievements.AddAsync(achievement, ct);
        }

        if (newAchievements.Any())
            await _uow.SaveChangesAsync(ct);

        // Map ngược lại Response kết quả trả về cho Client hiển thị (Popup chúc mừng)
        var responses = newAchievements
            .Select(a => new AchievementResponse(
                a.Id,
                definitions.First(d => d.Id == a.AchievementDefinitionId).Name,
                definitions.First(d => d.Id == a.AchievementDefinitionId).Description,
                definitions.First(d => d.Id == a.AchievementDefinitionId).BadgeUrl,
                definitions.First(d => d.Id == a.AchievementDefinitionId).Type,
                definitions.First(d => d.Id == a.AchievementDefinitionId).Threshold,
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

    public async Task<PagedResponse<AchievementDefinitionResponse>> GetAllDefinitionsPagedAsync(
        int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = _uow.AchievementDefinitions.Query()
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
            ));

        return await query.ToPagedResponseAsync(pageNumber, pageSize, ct);
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
        var existing = await _uow.AchievementDefinitions.Query()
            .FirstOrDefaultAsync(d => d.Type == request.Type && d.Threshold == request.Threshold, ct);

        if (existing != null && !existing.IsDeleted)
            throw new AppException($"Achievement definition with type '{request.Type}' and threshold {request.Threshold} already exists.");

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

        // Kiểm tra: Dựa trực tiếp vào FK mới để tìm xem đứa trẻ nào đã giữ định nghĩa này chưa
        var referencedAchievements = await _uow.Achievements.Query()
            .AnyAsync(a => a.AchievementDefinitionId == id, ct);

        if (referencedAchievements)
            throw new AppException("Cannot hard delete achievement definition that has been earned by children.");

        // [FIX #3] Dùng ExecuteDeleteAsync với await để đảm bảo lệnh xóa thực sự chạy và được chờ kết quả.
        // Trước đó ExecuteDelete() không có await nên lệnh xóa có thể không chạy mà vẫn trả true.
        await _uow.AchievementDefinitions.Query()
            .IgnoreQueryFilters()
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(ct);

        return true;
    }
}