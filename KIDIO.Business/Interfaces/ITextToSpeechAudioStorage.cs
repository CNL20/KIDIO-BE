namespace KIDIO.Business.Interfaces;

public interface ITextToSpeechAudioStorage
{
    Task<bool> ExistsAsync(string fileName, CancellationToken ct = default);
    Task<string> SaveAsync(string fileName, byte[] audioBytes, CancellationToken ct = default);
    string GetPublicUrl(string fileName);
}