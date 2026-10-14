namespace LangNghe.Domain.Entities;

/// <summary>
/// Thông báo in-app cho user (workflow, hệ thống).
/// Map với bảng Sys_Notifications.
/// </summary>
public class Notification
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public required string Title { get; set; }
    public string? Message { get; set; }
    public required string Type { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}
