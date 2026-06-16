namespace KIDIO.Business.DTOs.TextToSpeech;

public record TextToSpeechOptionsRequest
{
    public string Voice { get; init; } = "Jenny";
    public int Rate { get; init; } = 0;
    public int Pitch { get; init; } = 0;
}

public record TextToSpeechSynthesizeRequest : TextToSpeechOptionsRequest
{
    public string Text { get; init; } = string.Empty;
}

public record TextToSpeechVoiceResponse(
    string Code,
    string DisplayName,
    string AzureVoiceName,
    string Locale,
    string Gender,
    bool IsDefault
);

public record TextToSpeechGeneratedResponse(
    string AudioUrl,
    string FileName,
    string VoiceCode,
    string VoiceName,
    bool IsCached
);