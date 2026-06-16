using Microsoft.AspNetCore.Http;

namespace KIDIO.Business.DTOs.Pronunciation;

/// <summary>
/// Request to submit pronunciation audio for scoring
/// </summary>
public class SubmitPronunciationRequest
{
    /// <summary>
    /// Child ID
    /// </summary>
    public Guid ChildId { get; set; }

    /// <summary>
    /// Vocabulary ID to score pronunciation for
    /// </summary>
    public Guid VocabularyId { get; set; }

    /// <summary>
    /// Optional: Lesson ID for context
    /// </summary>
    public Guid? LessonId { get; set; }

    /// <summary>
    /// Audio file (WAV format only)
    /// </summary>
    public IFormFile AudioFile { get; set; } = null!;
}

/// <summary>
/// Pronunciation scoring response
/// </summary>
public class PronunciationScoreResponse
{
    /// <summary>
    /// Log record ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Vocabulary ID
    /// </summary>
    public Guid VocabularyId { get; set; }

    /// <summary>
    /// Word being scored
    /// </summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>
    /// Accuracy score (0-100)
    /// </summary>
    public int AccuracyScore { get; set; }

    /// <summary>
    /// Fluency score (0-100)
    /// </summary>
    public int FluencyScore { get; set; }

    /// <summary>
    /// Completeness score (0-100)
    /// </summary>
    public int CompletenessScore { get; set; }

    /// <summary>
    /// Overall score (0-100) - average of 3 metrics
    /// </summary>
    public int OverallScore { get; set; }

    /// <summary>
    /// Pass/Fail (true if OverallScore >= 70)
    /// </summary>
    public bool IsPassed { get; set; }

    /// <summary>
    /// Simple feedback for user
    /// </summary>
    public string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// Stored audio file URL
    /// </summary>
    public string AudioStorageUrl { get; set; } = string.Empty;

    /// <summary>
    /// When created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Pronunciation history response
/// </summary>
public class PronunciationHistoryResponse
{
    /// <summary>
    /// Vocabulary ID
    /// </summary>
    public Guid VocabularyId { get; set; }

    /// <summary>
    /// Word
    /// </summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>
    /// Total attempts
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Best score achieved
    /// </summary>
    public int BestScore { get; set; }

    /// <summary>
    /// Last attempt score
    /// </summary>
    public int LastAttemptScore { get; set; }

    /// <summary>
    /// Latest attempt
    /// </summary>
    public PronunciationScoreResponse? LastAttempt { get; set; }
}

/// <summary>
/// Paginated pronunciation history
/// </summary>
public class PagedPronunciationResponse
{
    /// <summary>
    /// List of pronunciation records
    /// </summary>
    public List<PronunciationScoreResponse> Items { get; set; } = new();

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Whether there's a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there's a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}
