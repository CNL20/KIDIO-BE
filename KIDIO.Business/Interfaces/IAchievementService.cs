using KIDIO.Common;
using KIDIO.Business.DTOs.Achievement;

namespace KIDIO.Business.Interfaces;

public interface IAchievementService
{
    Task<List<AchievementResponse>> GetByChildAsync(Guid childId, Guid parentId, CancellationToken ct = default);
    Task<PagedResponse<AchievementResponse>> GetByChildPagedAsync(Guid childId, Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<AchievementUnlockResult> CheckAndUnlockAsync(Guid childId, CancellationToken ct = default);

    // Admin methods for managing achievement definitions
    Task<List<AchievementDefinitionResponse>> GetActiveDefinitionsAsync(CancellationToken ct = default);
    Task<List<AchievementDefinitionResponse>> GetAllDefinitionsAsync(CancellationToken ct = default);
    Task<PagedResponse<AchievementDefinitionResponse>> GetAllDefinitionsPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<AchievementDefinitionResponse?> GetDefinitionByIdAsync(Guid id, CancellationToken ct = default);
    Task<AchievementDefinitionResponse> CreateDefinitionAsync(CreateAchievementDefinitionRequest request, CancellationToken ct = default);
    Task<AchievementDefinitionResponse?> UpdateDefinitionAsync(Guid id, UpdateAchievementDefinitionRequest request, CancellationToken ct = default);
    Task<bool> DeleteDefinitionAsync(Guid id, CancellationToken ct = default);
    Task<bool> RestoreDefinitionAsync(Guid id, CancellationToken ct = default);
    Task<bool> HardDeleteDefinitionAsync(Guid id, CancellationToken ct = default);
}