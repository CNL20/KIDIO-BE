using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KIDIO.Business.DTOs.TextToSpeech;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;

namespace KIDIO.Business.Services;

public class TextToSpeechService : ITextToSpeechService
{
    private static readonly IReadOnlyList<TextToSpeechVoiceResponse> Voices = new List<TextToSpeechVoiceResponse>
    {
        new("Jenny", "Jenny", "en-US-JennyNeural", "en-US", "Female", true),
        new("Ana", "Ana", "en-US-AnaNeural", "en-US", "Female", false),
        new("Guy", "Guy", "en-US-GuyNeural", "en-US", "Male", false),
        new("Sonia", "Sonia", "en-US-SoniaNeural", "en-US", "Female", false)
    };

    private readonly IUnitOfWork _uow;
    private readonly AzureSpeechSettings _settings;
    private readonly ITextToSpeechAudioStorage _audioStorage;

    public TextToSpeechService(
        IUnitOfWork uow,
        IOptions<AzureSpeechSettings> settings,
        ITextToSpeechAudioStorage audioStorage)
    {
        _uow = uow;
        _settings = settings.Value;
        _audioStorage = audioStorage;
    }

    public Task<List<TextToSpeechVoiceResponse>> GetVoicesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Voices.ToList());
    }

    public async Task<TextToSpeechGeneratedResponse> SynthesizeAsync(
        TextToSpeechSynthesizeRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            throw new AppException("Text cannot be empty.");

        var voice = ResolveVoice(request.Voice);
        var cacheKey = BuildCacheKey(request.Text, voice.Code, request.Rate.ToString(), request.Pitch.ToString());
        var fileName = $"tts-{cacheKey}.mp3";

        if (await _audioStorage.ExistsAsync(fileName, ct))
        {
            return new TextToSpeechGeneratedResponse(
                _audioStorage.GetPublicUrl(fileName),
                fileName,
                voice.Code,
                voice.AzureVoiceName,
                true);
        }

        var audioBytes = await SynthesizeToAudioBytesAsync(request.Text, voice, request.Rate, request.Pitch, ct);
        var audioUrl = await _audioStorage.SaveAsync(fileName, audioBytes, ct);

        return new TextToSpeechGeneratedResponse(
            audioUrl,
            fileName,
            voice.Code,
            voice.AzureVoiceName,
            false);
    }

    public async Task<TextToSpeechGeneratedResponse> SynthesizeLessonAsync(
        Guid lessonId, TextToSpeechOptionsRequest request, CancellationToken ct = default)
    {
        var lesson = await _uow.Lessons.GetByIdAsync(lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        var text = BuildLessonText(lesson);
        if (string.IsNullOrWhiteSpace(text))
            throw new AppException("Lesson content is empty. Cannot synthesize audio.");

        var voice = ResolveVoice(request.Voice);
        var cacheKey = BuildCacheKey(
            text,
            voice.Code,
            request.Rate.ToString(),
            request.Pitch.ToString(),
            lesson.UpdatedAt?.ToString("O") ?? lesson.CreatedAt.ToString("O"));
        var fileName = $"lesson-{lesson.Id}-{cacheKey}.mp3";

        if (await _audioStorage.ExistsAsync(fileName, ct))
        {
            return new TextToSpeechGeneratedResponse(
                _audioStorage.GetPublicUrl(fileName),
                fileName,
                voice.Code,
                voice.AzureVoiceName,
                true);
        }

        var audioBytes = await SynthesizeToAudioBytesAsync(text, voice, request.Rate, request.Pitch, ct);
        var audioUrl = await _audioStorage.SaveAsync(fileName, audioBytes, ct);

        return new TextToSpeechGeneratedResponse(
            audioUrl,
            fileName,
            voice.Code,
            voice.AzureVoiceName,
            false);
    }

    private async Task<byte[]> SynthesizeToAudioBytesAsync(
        string text,
        TextToSpeechVoiceResponse voice,
        int rate,
        int pitch,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_settings.AzureSpeechKey) || string.IsNullOrWhiteSpace(_settings.AzureSpeechRegion))
            throw new AppException("Azure Speech is not configured. Please set AzureSpeechKey and AzureSpeechRegion.");

        var speechConfig = SpeechConfig.FromSubscription(_settings.AzureSpeechKey, _settings.AzureSpeechRegion);
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);
        speechConfig.SpeechSynthesisVoiceName = voice.AzureVoiceName;

        var ssml = BuildSsml(text, voice.AzureVoiceName, voice.Locale, rate, pitch);

        using var synthesizer = new SpeechSynthesizer(speechConfig);
        var result = await synthesizer.SpeakSsmlAsync(ssml);

        if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            throw new AppException($"Azure Speech synthesis cancelled: {cancellation.Reason} - {cancellation.ErrorDetails}");
        }

        if (result.Reason != ResultReason.SynthesizingAudioCompleted || result.AudioData is null || result.AudioData.Length == 0)
            throw new AppException("Azure Speech did not return any audio data.");

        return result.AudioData;
    }

    private static TextToSpeechVoiceResponse ResolveVoice(string? voiceCode)
    {
        var normalized = string.IsNullOrWhiteSpace(voiceCode) ? "Jenny" : voiceCode.Trim();

        var voice = Voices.FirstOrDefault(v =>
            string.Equals(v.Code, normalized, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(v.AzureVoiceName, normalized, StringComparison.OrdinalIgnoreCase));

        if (voice is null)
            throw new AppException($"Unsupported voice '{voiceCode}'. Available voices: {string.Join(", ", Voices.Select(v => v.Code))}.");

        return voice;
    }

    private static string BuildSsml(
        string text,
        string voiceName,
        string locale,
        int rate,
        int pitch)
    {
        var safeText = SecurityElement.Escape(text) ?? string.Empty;

        return $"""
<speak version="1.0" xml:lang="{locale}">
  <voice name="{voiceName}">
    <prosody rate="{FormatProsodyValue(rate)}" pitch="{FormatProsodyValue(pitch)}">{safeText}</prosody>
  </voice>
</speak>
""";
    }

    private static string FormatProsodyValue(int value)
    {
        return value switch
        {
            0 => "0%",
            > 0 => $"+{value}%",
            _ => $"{value}%"
        };
    }

    private static string BuildLessonText(Lesson lesson)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(lesson.Title))
            parts.Add(lesson.Title);

        if (!string.IsNullOrWhiteSpace(lesson.Description))
            parts.Add(lesson.Description);

        if (!string.IsNullOrWhiteSpace(lesson.ContentJson))
        {
            try
            {
                using var document = JsonDocument.Parse(lesson.ContentJson);
                CollectText(document.RootElement, parts);
            }
            catch
            {
                parts.Add(lesson.ContentJson);
            }
        }

        return string.Join(" ", parts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
    }

    private static void CollectText(JsonElement element, List<string> parts, string? propertyName = null)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (TryBuildDialogueLine(element, out var dialogueLine))
                {
                    AddPart(parts, dialogueLine);
                    return;
                }

                foreach (var property in element.EnumerateObject())
                    CollectText(property.Value, parts, property.Name);
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectText(item, parts, propertyName);
                break;

            case JsonValueKind.String:
                if (ShouldIncludeProperty(propertyName))
                    AddPart(parts, element.GetString());
                break;
        }
    }

    private static bool TryBuildDialogueLine(JsonElement element, out string line)
    {
        line = string.Empty;

        if (TryGetStringProperty(element, "speaker", out var speaker) &&
            TryGetStringProperty(element, "text", out var text))
        {
            line = $"{speaker}: {text}";
            return true;
        }

        if (TryGetStringProperty(element, "role", out speaker) &&
            TryGetStringProperty(element, "message", out text))
        {
            line = $"{speaker}: {text}";
            return true;
        }

        return false;
    }

    private static bool TryGetStringProperty(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;

        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            return false;

        value = property.GetString()?.Trim() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool ShouldIncludeProperty(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return false;

        return propertyName.Equals("text", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("content", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("story", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("sentence", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("line", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("question", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("answer", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("prompt", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("description", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("title", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("narration", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("dialogue", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("dialogues", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("paragraph", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("paragraphs", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("sentences", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("word", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("meaning", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("value", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddPart(List<string> parts, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        parts.Add(text.Trim());
    }

    private static string BuildCacheKey(params string[] inputs)
    {
        var raw = string.Join("|", inputs.Select(x => x?.Trim() ?? string.Empty));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..24];
    }
}