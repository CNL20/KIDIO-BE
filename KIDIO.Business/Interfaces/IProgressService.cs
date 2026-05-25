using KIDIO.Business.DTOs.Progress;
using KIDIO.Common;

namespace KIDIO.Business.Interfaces;

public interface IProgressService
{
    Task<ProgressResponse> SubmitProgressAsync(Guid parentId, SubmitProgressRequest request, CancellationToken ct = default);
    Task<PagedResponse<ProgressResponse>> GetProgressByChildAsync(Guid childId, Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ProgressResponse?> GetLessonProgressAsync(Guid childId, Guid lessonId, Guid parentId, CancellationToken ct = default);
    Task<ChildProgressSummary> GetChildSummaryAsync(Guid childId, Guid parentId, CancellationToken ct = default);
    Task<PagedResponse<ProgressResponse>> GetRecentActivitiesAsync(Guid childId, Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResponse<ProgressResponse>> GetCompletedLessonsAsync(Guid childId, Guid parentId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
}