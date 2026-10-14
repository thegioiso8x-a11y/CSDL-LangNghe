namespace LangNghe.Application.Features.Notifications.Interfaces;

/// <summary>
/// Service tạo thông báo in-app.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Tạo 1 thông báo cho user.
    /// </summary>
    Task CreateAsync(int userId, string title, string? message, string type,
        string? entityType = null, string? entityId = null, string? linkUrl = null,
        CancellationToken ct = default);

    /// <summary>
    /// Tạo nhiều thông báo cùng lúc (VD notify tất cả user có role X).
    /// </summary>
    Task CreateBulkAsync(IEnumerable<int> userIds, string title, string? message, string type,
        string? entityType = null, string? entityId = null, string? linkUrl = null,
        CancellationToken ct = default);

    /// <summary>
    /// Đếm số chưa đọc của user.
    /// </summary>
    Task<int> CountUnreadAsync(int userId, CancellationToken ct = default);
}
