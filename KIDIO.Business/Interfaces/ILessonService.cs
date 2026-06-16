using KIDIO.Common;
using KIDIO.Business.DTOs.Lesson;

namespace KIDIO.Business.Interfaces;

public interface ILessonService
{
    Task<List<LessonSummaryResponse>> GetAllLessonsAsync(bool includeUnpublished = false, CancellationToken ct = default);
    Task<List<LessonSummaryResponse>> GetLessonsByTopicAsync(Guid topicId, bool includeUnpublished = false, CancellationToken ct = default);
    Task<PagedResponse<LessonSummaryResponse>> GetAllLessonsPagedAsync(bool includeUnpublished = false, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResponse<LessonSummaryResponse>> GetLessonsByTopicPagedAsync(Guid topicId, bool includeUnpublished = false, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<LessonResponse> GetLessonByIdAsync(Guid lessonId, bool includeUnpublished = false, CancellationToken ct = default);
    Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct = default);
    Task<LessonResponse> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request, CancellationToken ct = default);
    Task DeleteLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task PublishLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task UnpublishLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task RestoreLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task HardDeleteLessonAsync(Guid lessonId, CancellationToken ct = default);
}