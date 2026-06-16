using System.Text;
using System.Text.Json;
using KIDIO.Business.DTOs.Pronunciation;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KIDIO.Business.Services;

/// <summary>
/// Service for pronunciation scoring using Azure Speech Pronunciation Assessment
/// </summary>
public class PronunciationScoringService : IPronunciationScoringService
{
    private const int MinAudioLengthSeconds = 1;
    private const int MaxAudioLengthSeconds = 30;
    private const int MaxFileSizeKb = 10240; // 10MB
    private const int PassThreshold = 70;
    private const string AllowedAudioFormat = "wav";

    private readonly IUnitOfWork _uow;
    private readonly AzureSpeechSettings _azureSpeechSettings;
    private readonly IPronunciationAudioStorage _audioStorage;
    private readonly ILogger<PronunciationScoringService> _logger;

    public PronunciationScoringService(
        IUnitOfWork uow,
        IOptions<AzureSpeechSettings> azureSpeechOptions,
        IPronunciationAudioStorage audioStorage,
        ILogger<PronunciationScoringService> logger)
    {
        _uow = uow;
        _azureSpeechSettings = azureSpeechOptions.Value;
        _audioStorage = audioStorage;
        _logger = logger;
    }

    public async Task<PronunciationScoreResponse> SubmitPronunciationAsync(
        Guid childId,
        SubmitPronunciationRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Validate input
        ValidateAudioFile(request.AudioFile);

        // Get vocabulary
        var vocabulary = await _uow.Vocabularies.GetByIdAsync(request.VocabularyId, ct)
            ?? throw new NotFoundException("Vocabulary");

        _logger.LogInformation($"Child {childId} submitting pronunciation for vocabulary {vocabulary.Id} ({vocabulary.Word})");

        // Save audio file
        var audioFileName = await SaveAudioFileAsync(request.AudioFile, ct);
        var audioStorageUrl = _audioStorage.GetPublicUrl(audioFileName);

        try
        {
            // Score pronunciation using Azure
            var (accuracyScore, fluencyScore, completenessScore) = await ScorePronunciationAsync(vocabulary.Word, request.AudioFile, ct);

            // Calculate overall score and pass status
            var overallScore = (int)Math.Round((accuracyScore + fluencyScore + completenessScore) / 3.0);
            var isPassed = overallScore >= PassThreshold;

            // Generate detailed feedback
            var feedback = PronunciationFeedbackGenerator.GenerateFeedback(
                overallScore, accuracyScore, fluencyScore, completenessScore);

            // Create pronunciation log
            var pronunciationLog = new PronunciationLog
            {
                ChildId = childId,
                LessonId = request.LessonId,
                TargetText = vocabulary.Word,
                AudioStorageUrl = audioStorageUrl,
                AccuracyScore = accuracyScore,
                FluencyScore = fluencyScore,
                CompletenessScore = completenessScore,
                AiFeedbackJson = feedback
            };

            // Save to database
            await _uow.PronunciationLogs.AddAsync(pronunciationLog, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation($"Pronunciation logged for child {childId}: overall score {overallScore}");

            return MapToResponse(request.VocabularyId, pronunciationLog, vocabulary.Word);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error scoring pronunciation for child {childId}, vocabulary {request.VocabularyId}");
            throw;
        }
    }

    public async Task<PronunciationHistoryResponse> GetVocabularyHistoryAsync(
        Guid childId,
        Guid vocabularyId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var vocabulary = await _uow.Vocabularies.GetByIdAsync(vocabularyId, ct)
            ?? throw new NotFoundException("Vocabulary");

        var logs = await _uow.PronunciationLogs.Query()
            .Where(x => x.ChildId == childId && x.TargetText == vocabulary.Word)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var bestScore = logs.Any() ? (int)Math.Round((logs[0].AccuracyScore + logs[0].FluencyScore + logs[0].CompletenessScore) / 3.0) : 0;
        var lastAttempt = logs.FirstOrDefault();

        return new PronunciationHistoryResponse
        {
            VocabularyId = vocabularyId,
            Word = vocabulary.Word,
            AttemptCount = logs.Count,
            BestScore = logs.Any() ? logs.Max(x => (int)Math.Round((x.AccuracyScore + x.FluencyScore + x.CompletenessScore) / 3.0)) : 0,
            LastAttemptScore = logs.Any() ? (int)Math.Round((logs[0].AccuracyScore + logs[0].FluencyScore + logs[0].CompletenessScore) / 3.0) : 0,
            LastAttempt = lastAttempt != null ? MapToResponse(vocabularyId, lastAttempt, vocabulary.Word) : null
        };
    }

    public async Task<List<PronunciationScoreResponse>> GetChildHistoryAsync(
        Guid childId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var logs = await _uow.PronunciationLogs.Query()
            .Where(x => x.ChildId == childId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return logs.Select(log => MapToResponse(Guid.Empty, log, log.TargetText)).ToList();
    }

    public async Task<PagedPronunciationResponse> GetChildHistoryPagedAsync(
        Guid childId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _uow.PronunciationLogs.Query()
            .Where(x => x.ChildId == childId)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedPronunciationResponse
        {
            Items = items.Select(log => MapToResponse(Guid.Empty, log, log.TargetText)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // ─── Private Methods ────────────────────────────────────────────

    private void ValidateAudioFile(IFormFile audioFile)
    {
        if (audioFile == null || audioFile.Length == 0)
            throw new AppException("Audio file is required.");

        // Check file extension
        var fileName = audioFile.FileName.ToLower();
        if (!fileName.EndsWith(AllowedAudioFormat))
            throw new AppException($"Only {AllowedAudioFormat.ToUpper()} format is supported.");

        // Check file size (10MB)
        if (audioFile.Length > MaxFileSizeKb * 1024)
            throw new AppException($"Audio file size must be less than {MaxFileSizeKb / 1024}MB.");
    }

    private async Task<string> SaveAudioFileAsync(IFormFile audioFile, CancellationToken ct)
    {
        var fileName = $"pronunciation-{Guid.NewGuid():N}.wav";
        using (var stream = audioFile.OpenReadStream())
        {
            var audioBytes = new byte[stream.Length];
            await stream.ReadAsync(audioBytes, 0, (int)stream.Length, ct);
            await _audioStorage.SaveAsync(fileName, audioBytes, ct);
        }
        return fileName;
    }

    private async Task<(int AccuracyScore, int FluencyScore, int CompletenessScore)> ScorePronunciationAsync(
        string targetText,
        IFormFile audioFile,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_azureSpeechSettings.AzureSpeechKey) ||
            string.IsNullOrWhiteSpace(_azureSpeechSettings.AzureSpeechRegion))
            throw new AppException("Azure Speech is not configured.");

        try
        {
            // Use HTTP REST API for Pronunciation Assessment
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _azureSpeechSettings.AzureSpeechKey);

            // Add the required Pronunciation-Assessment header
            var pronAssessmentParamsJson = "{\"ReferenceText\":\"" + targetText + "\",\"GradingSystem\":\"HundredMark\",\"Dimension\":\"Comprehensive\",\"EnableMiscue\":false}";
            var pronAssessmentParamsBytes = Encoding.UTF8.GetBytes(pronAssessmentParamsJson);
            var pronAssessmentHeader = Convert.ToBase64String(pronAssessmentParamsBytes);
            httpClient.DefaultRequestHeaders.Add("Pronunciation-Assessment", pronAssessmentHeader);

            // Read audio file bytes
            using (var audioStream = audioFile.OpenReadStream())
            {
                var audioBytes = new byte[audioStream.Length];
                await audioStream.ReadAsync(audioBytes, 0, (int)audioStream.Length, ct);

                // Build request URL with pronunciation assessment parameters
                var region = _azureSpeechSettings.AzureSpeechRegion.ToLower();
                var url = $"https://{region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1" +
                    $"?language=en-US" +
                    $"&format=detailed";

                using (var content = new ByteArrayContent(audioBytes))
                {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

                    _logger.LogInformation($"Calling Azure Speech Recognition with Pronunciation Assessment for: {targetText}");

                    var response = await httpClient.PostAsync(url, content, ct);
                    var responseText = await response.Content.ReadAsStringAsync(ct);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Azure Speech API error: {response.StatusCode} - {responseText}");
                        throw new AppException($"Azure Speech API error: {response.StatusCode}");
                    }

                    // Parse response and extract scores
                    var result = ParseSpeechRecognitionResponse(responseText, targetText);
                    return result;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling Azure Speech API");
            throw new AppException("Network error contacting Azure Speech service. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error scoring pronunciation for '{targetText}'");
            throw;
        }
    }

    private (int AccuracyScore, int FluencyScore, int CompletenessScore) ParseSpeechRecognitionResponse(string jsonResponse, string targetText)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            // Check if recognition was successful
            if (root.TryGetProperty("RecognitionStatus", out var statusElem))
            {
                var status = statusElem.GetString();
                if (status != "Success")
                {
                    _logger.LogWarning($"Azure Speech recognition status: {status}");
                    // Return moderate scores if recognition not perfect
                    return (60, 60, 60);
                }
            }

            // Try to get PronunciationAssessment from response
            var accuracyScore = 75;
            var fluencyScore = 75;
            var completenessScore = 75;

            if (root.TryGetProperty("NBest", out var nbestElem) && nbestElem.ValueKind == JsonValueKind.Array && nbestElem.GetArrayLength() > 0)
            {
                var bestResult = nbestElem[0];
                if (bestResult.TryGetProperty("AccuracyScore", out var accElem))
                    accuracyScore = (int)Math.Round(accElem.GetDouble());
                if (bestResult.TryGetProperty("FluencyScore", out var fluElem))
                    fluencyScore = (int)Math.Round(fluElem.GetDouble());
                if (bestResult.TryGetProperty("CompletenessScore", out var comElem))
                    completenessScore = (int)Math.Round(comElem.GetDouble());
            }

            _logger.LogInformation(
                $"Pronunciation assessment for '{targetText}' - " +
                $"Accuracy: {accuracyScore}, Fluency: {fluencyScore}, Completeness: {completenessScore}");

            return (accuracyScore, fluencyScore, completenessScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Azure Speech response");
            // Return moderate scores if parsing fails
            return (70, 70, 70);
        }
    }

    private PronunciationScoreResponse MapToResponse(Guid vocabularyId, PronunciationLog log, string word)
    {
        var overallScore = (int)Math.Round((log.AccuracyScore + log.FluencyScore + log.CompletenessScore) / 3.0);
        return new PronunciationScoreResponse
        {
            Id = log.Id,
            VocabularyId = vocabularyId,
            Word = word,
            AccuracyScore = log.AccuracyScore,
            FluencyScore = log.FluencyScore,
            CompletenessScore = log.CompletenessScore,
            OverallScore = overallScore,
            IsPassed = overallScore >= PassThreshold,
            Feedback = log.AiFeedbackJson ?? "No feedback available.",
            AudioStorageUrl = log.AudioStorageUrl ?? "",
            CreatedAt = log.CreatedAt
        };
    }
}

