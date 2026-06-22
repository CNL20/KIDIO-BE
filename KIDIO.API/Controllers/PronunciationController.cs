using KIDIO.Business.DTOs.Pronunciation;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PronunciationController : ControllerBase
{
    private readonly IPronunciationScoringService _pronunciationService;
    private readonly ILogger<PronunciationController> _logger;

    public PronunciationController(
        IPronunciationScoringService pronunciationService,
        ILogger<PronunciationController> logger)
    {
        _pronunciationService = pronunciationService;
        _logger = logger;
    }

    /// <summary>
    /// Submit pronunciation audio for scoring
    /// </summary>
    /// <remarks>
    /// Upload a WAV file to score pronunciation of a vocabulary word.
    /// Supported format: WAV only, max 10MB.
    /// </remarks>
    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<PronunciationScoreResponse>>> SubmitPronunciation(
        [FromForm] SubmitPronunciationRequest request,
        CancellationToken ct)
    {
        // ExceptionMiddleware xử lý AppException, NotFoundException, v.v. — không cần try/catch thủ công
        var result = await _pronunciationService.SubmitPronunciationAsync(
            request.ChildId,
            request,
            ct);

        return Ok(ApiResponse<PronunciationScoreResponse>.Ok(result, "Pronunciation scored successfully."));
    }

    /// <summary>
    /// Get pronunciation history for a specific vocabulary
    /// </summary>
    [HttpGet("vocabulary/{vocabularyId:guid}")]
    public async Task<ActionResult<ApiResponse<PronunciationHistoryResponse>>> GetVocabularyHistory(
        Guid vocabularyId,
        CancellationToken ct)
    {
        var result = await _pronunciationService.GetVocabularyHistoryAsync(
            GetCurrentUserId(),
            vocabularyId,
            ct);

        return Ok(ApiResponse<PronunciationHistoryResponse>.Ok(result));
    }

    /// <summary>
    /// Get all pronunciation logs for current child with pagination
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<PagedPronunciationResponse>>> GetChildHistoryPaged(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _pronunciationService.GetChildHistoryPagedAsync(
            GetCurrentUserId(),
            pageNumber,
            pageSize,
            ct);

        return Ok(ApiResponse<PagedPronunciationResponse>.Ok(result));
    }

    // ─── Helper ────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(value);
    }
}
