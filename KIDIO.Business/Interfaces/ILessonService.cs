using KIDIO.Business.DTOs.Lesson;

namespace KIDIO.Business.Interfaces;

public interface ILessonService
{
    Task<List<LessonSummaryResponse>> GetAllLessonsAsync(bool includeUnpublished = false, CancellationToken ct = default);
    Task<List<LessonSummaryResponse>> GetLessonsByTopicAsync(Guid topicId, bool includeUnpublished = false, CancellationToken ct = default);
    Task<LessonResponse> GetLessonByIdAsync(Guid lessonId, bool includeUnpublished = false, CancellationToken ct = default);
    Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct = default);
    Task<LessonResponse> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request, CancellationToken ct = default);
    Task DeleteLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task PublishLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task UnpublishLessonAsync(Guid lessonId, CancellationToken ct = default);
}