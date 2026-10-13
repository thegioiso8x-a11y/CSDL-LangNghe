namespace LangNghe.Domain.Entities;

/// <summary>
/// Đơn vị tính sản lượng.
/// Map với bảng Dim_OutputUnits.
/// </summary>
public class OutputUnit
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
}