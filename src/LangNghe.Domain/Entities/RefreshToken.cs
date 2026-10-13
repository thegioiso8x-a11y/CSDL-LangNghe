namespace LangNghe.Domain.Entities;

/// <summary>
/// Lưu trữ Refresh Token của người dùng.
/// </summary>
public class RefreshToken
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public string? ReplacedBy { get; set; }

    public User? User { get; set; }
}
