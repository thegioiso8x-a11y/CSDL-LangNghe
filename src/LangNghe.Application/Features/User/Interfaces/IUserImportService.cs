using LangNghe.Application.Features.User.Dtos;

namespace LangNghe.Application.Features.User.Interfaces;

/// <summary>Service phân tích, xem trước và import file người dùng.</summary>
public interface IUserImportService
{
    Task<UserImportPreviewDto> PreviewAsync(Stream stream, string importMode, CancellationToken cancellationToken);
    Task<UserImportResultDto> ImportAsync(Stream stream, string fileName, long fileSizeBytes, string importMode, bool skipErrors, CancellationToken cancellationToken);
    byte[] CreateTemplate();
}
