using System;
using LangNghe.Domain.Entities.Common;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Lưu trữ lịch sử sáp nhập, đổi tên địa giới hành chính.
/// Map với bảng Dim_RegionMapping.
/// </summary>
public class RegionMapping : BaseEntity
{
    public required string OldCode { get; set; }
    public required string NewCode { get; set; }
    public string? OldName { get; set; }
    public string? NewName { get; set; }
    public byte LevelType { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? DecreeNumber { get; set; }
    public string? Notes { get; set; }
}