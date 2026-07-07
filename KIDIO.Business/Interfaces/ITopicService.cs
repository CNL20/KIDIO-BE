using KIDIO.Common;
using KIDIO.Business.DTOs.Lesson;

namespace KIDIO.Business.Interfaces;

public interface ITopicService
{
    Task<List<TopicSummaryResponse>> GetAllTopicsAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<PagedResponse<TopicSummaryResponse>> GetTopicsPagedAsync(int pageNumber = 1, int pageSize = 10, bool includeInactive = false, CancellationToken ct = default);
    Task<TopicResponse> GetTopicByIdAsync(Guid topicId, CancellationToken ct = default);
    Task<TopicResponse> CreateTopicAsync(CreateTopicRequest request, CancellationToken ct = default);
    Task<TopicResponse> UpdateTopicAsync(Guid topicId, UpdateTopicRequest request, CancellationToken ct = default);
    Task DeleteTopicAsync(Guid topicId, CancellationToken ct = default);
    Task RestoreTopicAsync(Guid topicId, CancellationToken ct = default);
    Task HardDeleteTopicAsync(Guid topicId, CancellationToken ct = default);
}