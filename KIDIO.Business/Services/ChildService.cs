using KIDIO.Business.DTOs.Child;
using KIDIO.Business.Extensions;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class ChildService : IChildService
{
    private readonly IUnitOfWork _uow;
    private const int MaxChildrenPerParent = 5;

    public ChildService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<ChildSummaryResponse>> GetChildrenByParentAsync(
        Guid parentId, CancellationToken ct = default)
    {
        var children = await _uow.Children.FindAsync(
            c => c.ParentId == parentId, ct);

        return children
            .OrderBy(c => c.CreatedAt)
            .Select(MapToSummary)
            .ToList();
    }

    public async Task<PagedResponse<ChildSummaryResponse>> GetChildrenByParentPagedAsync(
        Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = _uow.Children.Query()
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new ChildSummaryResponse(
                c.Id,
                c.Name,
                c.Age,
                c.AvatarUrl,
                c.TotalStars,
                c.CurrentStreakDays
            ));

        return await query.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<ChildResponse> GetChildByIdAsync(
        Guid childId, Guid parentId, CancellationToken ct = default)
    {
        var child = await GetAndVerifyOwnershipAsync(childId, parentId, ct);
        return MapToResponse(child);
    }

    public async Task<ChildResponse> CreateChildAsync(
        Guid parentId, CreateChildRequest request, CancellationToken ct = default)
    {
        _ = await _uow.Users.GetByIdAsync(parentId, ct)
            ?? throw new NotFoundException("Parent");

        var existing = await _uow.Children.FindAsync(
            c => c.ParentId == parentId, ct);

        if (existing.Count() >= MaxChildrenPerParent)
            throw new AppException(
                $"Maximum {MaxChildrenPerParent} children profiles allowed per account.");

        ValidateAge(request.Age);

        var child = new Child
        {
            Name = request.Name.Trim(),
            Age = request.Age,
            AvatarUrl = request.AvatarUrl,
            ParentId = parentId
        };

        await _uow.Children.AddAsync(child, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToResponse(child);
    }

    public async Task<ChildResponse> UpdateChildAsync(
        Guid childId, Guid parentId, UpdateChildRequest request, CancellationToken ct = default)
    {
        var child = await GetAndVerifyOwnershipAsync(childId, parentId, ct);

        ValidateAge(request.Age);

        child.Name = request.Name.Trim();
        child.Age = request.Age;
        child.AvatarUrl = request.AvatarUrl;

        _uow.Children.Update(child);
        await _uow.SaveChangesAsync(ct);

        return MapToResponse(child);
    }

    public async Task DeleteChildAsync(
        Guid childId, Guid parentId, CancellationToken ct = default)
    {
        var child = await GetAndVerifyOwnershipAsync(childId, parentId, ct);

        child.IsDeleted = true;

        _uow.Children.Update(child);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RestoreChildAsync(Guid childId, Guid parentId, CancellationToken ct = default)
    {
        var child = await _uow.Children.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == childId, ct)
            ?? throw new NotFoundException("Child");

        if (child.ParentId != parentId)
            throw new ForbiddenException("You do not have access to this child profile.");

        if (!child.IsDeleted)
            throw new AppException("Child profile is not deleted.");

        child.IsDeleted = false;
        _uow.Children.Update(child);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task HardDeleteChildAsync(Guid childId, CancellationToken ct = default)
    {
        var child = await _uow.Children.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == childId, ct)
            ?? throw new NotFoundException("Child");

        _uow.Children.Remove(child);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Helpers ─────────────────────────────────────────────

    private async Task<Child> GetAndVerifyOwnershipAsync(
        Guid childId, Guid parentId, CancellationToken ct)
    {
        var child = await _uow.Children.GetByIdAsync(childId, ct)
            ?? throw new NotFoundException("Child");

        if (child.ParentId != parentId)
            throw new ForbiddenException("You do not have access to this child profile.");

        return child;
    }

    private static void ValidateAge(int age)
    {
        // [FIX #5] Thu hẹp giới hạn tuổi từ 1-150 xuống 1-18 cho phù hợp mục đích giáo dục trẻ em.
        // Frontend dùng giới hạn 4-10 cho UI; backend dùng 1-18 để làm safety net rộng hơn một chút.
        if (age < 1 || age > 18)
            throw new AppException("Age must be between 1 and 18.");
    }

    private static ChildSummaryResponse MapToSummary(Child c) => new(
        Id: c.Id,
        Name: c.Name,
        Age: c.Age,
        AvatarUrl: c.AvatarUrl,
        TotalStars: c.TotalStars,
        CurrentStreakDays: c.CurrentStreakDays
    );

    private static ChildResponse MapToResponse(Child c) => new(
        Id: c.Id,
        Name: c.Name,
        Age: c.Age,
        AvatarUrl: c.AvatarUrl,
        TotalStars: c.TotalStars,
        CurrentStreakDays: c.CurrentStreakDays,
        LastLessonAt: c.LastLessonAt,
        CreatedAt: c.CreatedAt,
        IsRecommendedAge: c.Age <= 10   // ← chỉ là thông tin, không phải rule
    );
}