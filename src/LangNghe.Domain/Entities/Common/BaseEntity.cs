using System;

namespace LangNghe.Domain.Entities.Common;

/// <summary>
/// Lớp cơ sở cho các entity có cấu trúc khóa chính là số nguyên (Id) và có trường ngày tạo (CreatedAt).
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Khóa chính của bảng.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Thời gian tạo dòng dữ liệu.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}