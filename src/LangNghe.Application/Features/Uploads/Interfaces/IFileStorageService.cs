namespace LangNghe.Application.Features.Uploads.Interfaces;

/// <summary>
/// Service lưu trữ file upload (ảnh).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Lưu file ảnh vào storage. Trả về URL tương đối (VD /uploads/villages/2026/10/abc.jpg).
    /// </summary>
    Task<string> SaveImageAsync(Stream stream, string originalFileName, string category, CancellationToken ct = default);

    /// <summary>
    /// Xóa file ảnh theo URL tương đối. Không lỗi nếu file không tồn tại.
    /// </summary>
    Task DeleteAsync(string relativeUrl, CancellationToken ct = default);
}
