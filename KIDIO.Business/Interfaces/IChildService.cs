using KIDIO.Business.DTOs.Child;

namespace KIDIO.Business.Interfaces;

public interface IChildService
{
    Task<List<ChildSummaryResponse>> GetChildrenByParentAsync(Guid parentId, CancellationToken ct = default);
    Task<ChildResponse> GetChildByIdAsync(Guid childId, Guid parentId, CancellationToken ct = default);
    Task<ChildResponse> CreateChildAsync(Guid parentId, CreateChildRequest request, CancellationToken ct = default);
    Task<ChildResponse> UpdateChildAsync(Guid childId, Guid parentId, UpdateChildRequest request, CancellationToken ct = default);
    Task DeleteChildAsync(Guid childId, Guid parentId, CancellationToken ct = default);
}