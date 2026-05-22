using KIDIO.Business.DTOs.Progress;

namespace KIDIO.Business.Interfaces;

public interface IProgressService
{
    Task<ProgressResponse> SubmitProgressAsync(Guid parentId, SubmitProgressRequest request, CancellationToken ct = default);
    Task<List<ProgressResponse>> GetProgressByChildAsync(Guid childId, Guid parentId, CancellationToken ct = default);
    Task<ProgressResponse?> GetLessonProgressAsync(Guid childId, Guid lessonId, Guid parentId, CancellationToken ct = default);
    Task<ChildProgressSummary> GetChildSummaryAsync(Guid childId, Guid parentId, CancellationToken ct = default);
    Task<List<ProgressResponse>> GetRecentActivitiesAsync(Guid childId, Guid parentId, int count = 5, CancellationToken ct = default);
    Task<List<ProgressResponse>> GetCompletedLessonsAsync(Guid childId, Guid parentId, CancellationToken ct = default);
}