namespace LangNghe.Domain.Entities;

/// <summary>
/// Nhóm mặt hàng xuất khẩu.
/// Map với bảng Dim_ExportCategories.
/// </summary>
public class ExportCategory
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
}