namespace LangNghe.Infrastructure.Services;

using LangNghe.Application.Features.Uploads.Interfaces;
using LangNghe.Domain.Exceptions;
using Microsoft.AspNetCore.Hosting;

/// <summary>
/// Lưu file vào wwwroot/uploads/{category}/{yyyy}/{MM}/{guid}.{ext}.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveImageAsync(Stream stream, string originalFileName, string category, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new BusinessException($"Định dạng ảnh không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedExtensions)}");

        if (stream.Length > MaxFileSize)
            throw new BusinessException("Kích thước ảnh vượt quá 5 MB.");

        // Sanitize category để tránh path traversal
        var safeCategory = new string(category.Where(c => char.IsLetterOrDigit(c)).ToArray());
        if (string.IsNullOrEmpty(safeCategory)) safeCategory = "misc";

        var now = DateTime.UtcNow;
        var relativeDir = Path.Combine("uploads", safeCategory, now.ToString("yyyy"), now.ToString("MM"));
        var absoluteDir = Path.Combine(_env.WebRootPath, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absolutePath = Path.Combine(absoluteDir, fileName);

        using (var fs = new FileStream(absolutePath, FileMode.Create))
        {
            await stream.CopyToAsync(fs, ct);
        }

        // Trả về URL tương đối, dùng / làm separator
        return $"/{relativeDir.Replace('\\', '/')}/{fileName}";
    }

    public Task DeleteAsync(string relativeUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl)) return Task.CompletedTask;
        if (!relativeUrl.StartsWith("/uploads/")) return Task.CompletedTask;

        var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(_env.WebRootPath, relativePath);

        if (File.Exists(absolutePath))
        {
            try { File.Delete(absolutePath); }
            catch { /* ignore */ }
        }

        return Task.CompletedTask;
    }
}
