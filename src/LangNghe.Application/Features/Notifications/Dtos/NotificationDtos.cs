namespace LangNghe.Application.Features.Notifications.Dtos;

using LangNghe.Application.Common;

/// <summary>
/// DTO thông tin một thông báo in-app.
/// </summary>
public record NotificationDto(
    long Id, string Title, string? Message, string Type,
    string? EntityType, string? EntityId, string? LinkUrl,
    bool IsRead, DateTime? ReadAt, DateTime CreatedAt);

/// <summary>
/// Bộ lọc và tham số phân trang danh sách thông báo.
/// </summary>
public record NotificationFilterDto : PageRequest
{
    /// <summary>Lọc theo trạng thái đã đọc.</summary>
    public bool? IsRead { get; set; }

    /// <summary>Lọc theo loại thông báo.</summary>
    public string? Type { get; set; }
}
