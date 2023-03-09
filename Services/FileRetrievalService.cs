using System.Diagnostics.CodeAnalysis;

namespace AssetServer.Services;

public class FileRetrievalService
{
    private readonly ILogger<FileRetrievalService> logger;

    public FileRetrievalService(ILogger<FileRetrievalService> logger)
    {
        this.logger = logger;
    }

    public bool TryLoadFile(string path, [NotNullWhen(true)] out FileStream? stream)
    {
        stream = null;

        if (!Path.Exists(path))
        {
            this.logger.LogWarning("Could not access requested resource at {path}", path);
            return false;
        }

        stream = File.OpenRead(path);
        return true;
    }
}
