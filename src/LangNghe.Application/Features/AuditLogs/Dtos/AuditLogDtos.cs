namespace LangNghe.Application.Features.AuditLogs.Dtos;

using System;
using LangNghe.Application.Common;

/// <summary>
/// DTO chứa thông tin chi tiết của một bản ghi nhật ký kiểm toán (Audit Log).
/// </summary>
public record AuditLogDto(
    long Id,
    int? UserId,
    string? Username,
    string Action,
    string EntityName,
    string? EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt);

/// <summary>
/// DTO chứa các tham số lọc và phân trang danh sách nhật ký kiểm toán.
/// </summary>
public record AuditLogFilterDto : PageRequest
{
    /// <summary>Lọc theo mã người dùng.</summary>
    public int? UserId { get; set; }

    /// <summary>Lọc theo hành động (Create, Update, Delete).</summary>
    public string? Action { get; set; }

    /// <summary>Lọc theo tên đối tượng/entity.</summary>
    public string? EntityName { get; set; }

    /// <summary>Lọc theo thời gian từ ngày.</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>Lọc theo thời gian đến ngày.</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>Từ khóa tìm kiếm (EntityId hoặc Username).</summary>
    public string? Keyword { get; set; }
}
