using KIDIO.Business.Interfaces;

namespace KIDIO.API.Services;

/// <summary>
/// Local file storage for pronunciation audio files
/// </summary>
public class LocalPronunciationAudioStorage : IPronunciationAudioStorage
{
    private const string PublicFolder = "pronunciations";
    private readonly string _rootPath;

    public LocalPronunciationAudioStorage(IWebHostEnvironment environment)
    {
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _rootPath = Path.Combine(webRoot, PublicFolder);
        Directory.CreateDirectory(_rootPath);
    }

    public Task<bool> ExistsAsync(string fileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(GetFullPath(fileName)));
    }

    public async Task<string> SaveAsync(string fileName, byte[] audioBytes, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var fullPath = GetFullPath(fileName);
        if (!File.Exists(fullPath))
        {
            await File.WriteAllBytesAsync(fullPath, audioBytes, ct);
        }

        return GetPublicUrl(fileName);
    }

    public string GetPublicUrl(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        return $"/{PublicFolder}/{safeFileName}";
    }

    private string GetFullPath(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        return Path.Combine(_rootPath, safeFileName);
    }
}
