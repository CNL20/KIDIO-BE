using KIDIO.Business.DTOs.TextToSpeech;

namespace KIDIO.Business.Interfaces;

public interface ITextToSpeechService
{
    Task<List<TextToSpeechVoiceResponse>> GetVoicesAsync(CancellationToken ct = default);
    Task<TextToSpeechGeneratedResponse> SynthesizeAsync(TextToSpeechSynthesizeRequest request, CancellationToken ct = default);
    Task<TextToSpeechGeneratedResponse> SynthesizeLessonAsync(Guid lessonId, TextToSpeechOptionsRequest request, CancellationToken ct = default);
}