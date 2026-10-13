namespace LangNghe.Domain.Entities;

/// <summary>
/// Dữ liệu tổng hợp theo thời gian của Làng nghề (cho Dashboard).
/// Map với bảng Agg_Village_TimeSeries.
/// Khóa chính (RegionId, Period, PeriodType).
/// </summary>
public class VillageTimeSeries
{
    public int RegionId { get; set; }
    public byte RegionLevel { get; set; }
    public int? ParentRegionId { get; set; }
    public required string Period { get; set; }
    public byte PeriodType { get; set; }
    public int TotalVillages { get; set; }
    public int TraditionalVillages { get; set; }
    public int NewVillages { get; set; }
    public int RestoredVillages { get; set; }
    public int OcopVillages { get; set; }
    public int EffectiveVillages { get; set; }
    public int TotalHouseholds { get; set; }
    public int TotalEnterprises { get; set; }
    public long TotalLabor { get; set; }
    public long TrainedLabor { get; set; }
    public int TotalArtisans { get; set; }
    public decimal? TotalArea { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExportValue { get; set; }
    public decimal? AvgRevenuePerVillage { get; set; }
    public decimal? AvgIncome { get; set; }
    public decimal? AvgGrowthRate { get; set; }
    public int DigitalizedVillages { get; set; }
    public int EnvCompliantVillages { get; set; }
    public int TourismVillages { get; set; }
    public long TotalVisitors { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? CalculatedBy { get; set; }

    // Navigation properties
    public Region? Region { get; set; }
}