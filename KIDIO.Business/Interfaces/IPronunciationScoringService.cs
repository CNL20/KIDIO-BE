using KIDIO.Business.DTOs.Pronunciation;

namespace KIDIO.Business.Interfaces;

/// <summary>
/// Service for pronunciation scoring using Azure Speech API
/// </summary>
public interface IPronunciationScoringService
{
    /// <summary>
    /// Submit pronunciation audio for scoring
    /// </summary>
    /// <param name="childId">Child ID</param>
    /// <param name="request">Submit request with audio file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Pronunciation score response</returns>
    Task<PronunciationScoreResponse> SubmitPronunciationAsync(
        Guid childId,
        SubmitPronunciationRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Get pronunciation history for a vocabulary
    /// </summary>
    /// <param name="childId">Child ID</param>
    /// <param name="vocabularyId">Vocabulary ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Pronunciation history</returns>
    Task<PronunciationHistoryResponse> GetVocabularyHistoryAsync(
        Guid childId,
        Guid vocabularyId,
        CancellationToken ct = default);

    /// <summary>
    /// Get all pronunciation logs for a child with pagination
    /// </summary>
    /// <param name="childId">Child ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged pronunciation records</returns>
    Task<PagedPronunciationResponse> GetChildHistoryPagedAsync(
        Guid childId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default);
}
