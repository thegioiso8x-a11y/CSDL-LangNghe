namespace LangNghe.Domain.Entities;

/// <summary>
/// Dữ liệu tổng hợp theo Loại hình làng nghề.
/// Map với bảng Agg_By_VillageType.
/// Khóa chính (VillageTypeId, RegionId, Period, PeriodType).
/// </summary>
public class VillageTypeAggregate
{
    public int VillageTypeId { get; set; }
    public int RegionId { get; set; }
    public byte RegionLevel { get; set; }
    public required string Period { get; set; }
    public byte PeriodType { get; set; }
    public int TotalVillages { get; set; }
    public decimal TotalRevenue { get; set; }
    public long TotalLabor { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public VillageType? VillageType { get; set; }
}