using KIDIO.Business.DTOs.Lesson;

namespace KIDIO.Business.Interfaces;

public interface ITopicService
{
    Task<List<TopicSummaryResponse>> GetAllTopicsAsync(CancellationToken ct = default);
    Task<TopicResponse> GetTopicByIdAsync(Guid topicId, CancellationToken ct = default);
    Task<TopicResponse> CreateTopicAsync(CreateTopicRequest request, CancellationToken ct = default);
    Task<TopicResponse> UpdateTopicAsync(Guid topicId, UpdateTopicRequest request, CancellationToken ct = default);
    Task DeleteTopicAsync(Guid topicId, CancellationToken ct = default);
}