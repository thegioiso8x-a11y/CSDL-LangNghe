using System;

namespace LangNghe.Domain.Interfaces;

/// <summary>
/// Interface cho các Entity cần theo dõi dấu vết CUD (Create, Update).
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Thời điểm tạo.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// ID của User đã tạo (có thể null nếu được tạo tự động bởi hệ thống).
    /// </summary>
    int? CreatedBy { get; set; }

    /// <summary>
    /// Thời điểm cập nhật cuối cùng.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// ID của User đã cập nhật dữ liệu.
    /// </summary>
    int? UpdatedBy { get; set; }
}