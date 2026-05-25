namespace KIDIO.Business.Interfaces;

/// <summary>
/// Interface for storing and retrieving pronunciation audio files
/// </summary>
public interface IPronunciationAudioStorage
{
    /// <summary>
    /// Check if file exists
    /// </summary>
    Task<bool> ExistsAsync(string fileName, CancellationToken ct = default);

    /// <summary>
    /// Save audio bytes to storage
    /// </summary>
    /// <returns>Public URL of saved file</returns>
    Task<string> SaveAsync(string fileName, byte[] audioBytes, CancellationToken ct = default);

    /// <summary>
    /// Get public URL for a file
    /// </summary>
    string GetPublicUrl(string fileName);
}
