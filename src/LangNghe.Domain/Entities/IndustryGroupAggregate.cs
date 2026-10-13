namespace LangNghe.Domain.Entities;

/// <summary>
/// Dữ liệu tổng hợp theo Nhóm ngành.
/// Map với bảng Agg_By_IndustryGroup.
/// Khóa chính (IndustryGroupId, RegionId, Period, PeriodType).
/// </summary>
public class IndustryGroupAggregate
{
    public int IndustryGroupId { get; set; }
    public int RegionId { get; set; }
    public byte RegionLevel { get; set; }
    public required string Period { get; set; }
    public byte PeriodType { get; set; }
    public int TotalVillages { get; set; }
    public decimal TotalRevenue { get; set; }
    public long TotalLabor { get; set; }
    public decimal TotalExportValue { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public IndustryGroup? IndustryGroup { get; set; }
}