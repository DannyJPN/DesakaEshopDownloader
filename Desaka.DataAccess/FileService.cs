namespace Desaka.DataAccess;

public sealed record FileWriteResult(string FullPath, string RelativePath, long Size, string Hash);

public sealed record FileReadResult(string FullPath, long Size, string Hash, byte[] Content);

public interface IFileService
{
    Task<FileWriteResult> WriteBytesAsync(string rootPath, string relativePath, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default);
    Task<FileWriteResult> WriteTextAsync(string rootPath, string relativePath, string content, CancellationToken cancellationToken = default);
    Task<FileReadResult> ReadBytesAsync(string rootPath, string relativePath, CancellationToken cancellationToken = default);
    string CombinePath(string rootPath, string relativePath);
}

public sealed class FileService : IFileService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".json", ".jpg", ".jpeg", ".png", ".bmp", ".txt", ".log", ".csv", ".xls", ".xlsx"
    };

    public string CombinePath(string rootPath, string relativePath)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        var combined = Path.GetFullPath(Path.Combine(fullRoot, relativePath));
        if (!combined.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path traversal detected.");
        }

        return combined;
    }

    public async Task<FileWriteResult> WriteBytesAsync(string rootPath, string relativePath, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
    {
        var path = CombinePath(rootPath, relativePath);
        ValidateExtension(path);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(path, content.ToArray(), cancellationToken);
        var hash = ComputeHash(content.Span);
        return new FileWriteResult(path, relativePath, content.Length, hash);
    }

    public async Task<FileWriteResult> WriteTextAsync(string rootPath, string relativePath, string content, CancellationToken cancellationToken = default)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty);
        return await WriteBytesAsync(rootPath, relativePath, bytes, cancellationToken);
    }

    public async Task<FileReadResult> ReadBytesAsync(string rootPath, string relativePath, CancellationToken cancellationToken = default)
    {
        var path = CombinePath(rootPath, relativePath);
        ValidateExtension(path);
        var content = await File.ReadAllBytesAsync(path, cancellationToken);
        var hash = ComputeHash(content);
        return new FileReadResult(path, content.Length, hash, content);
    }

    private static void ValidateExtension(string path)
    {
        var ext = Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new InvalidOperationException($"File extension '{ext}' is not allowed.");
        }
    }

    private static string ComputeHash(ReadOnlySpan<byte> bytes)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
