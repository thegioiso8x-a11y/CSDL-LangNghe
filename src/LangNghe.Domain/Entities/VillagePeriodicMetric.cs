using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// 47 chỉ tiêu định kỳ của làng nghề (Partition by year, Columnstore).
/// PK phức hợp, không dùng BaseEntity.
/// Map với bảng Fact_Village_PeriodicMetrics.
/// </summary>
public class VillagePeriodicMetric
{
    public long Id { get; set; }
    public int CraftVillageId { get; set; }
    public int CommuneId { get; set; }
    public int ProvinceId { get; set; }
    public required string ReportPeriod { get; set; }
    public byte PeriodType { get; set; }
    public Guid? ImportBatchId { get; set; }
    
    public bool BrandProtection { get; set; }
    public bool HasOCOP3Star { get; set; }
    public string? ProductionProcess { get; set; }
    public decimal ExportRate { get; set; }
    public int? ExportCategoryId { get; set; }
    public decimal ImportMaterialRate { get; set; }
    
    public int HouseholdCount { get; set; }
    public int EnterpriseCount { get; set; }
    public decimal? ProductionArea { get; set; }
    public byte? ActiveMonths { get; set; }
    
    public decimal? Output { get; set; }
    public int? OutputUnitId { get; set; }
    public decimal Revenue { get; set; }
    public decimal? GrowthRate { get; set; }
    
    public int TotalLabor { get; set; }
    public decimal? TrainedLaborRate { get; set; }
    public int ArtisanCount { get; set; }
    public string? TrainingNeed { get; set; }
    public decimal? AvgIncome { get; set; }
    
    public decimal? DigitalLevel { get; set; }
    public bool HasWebsite { get; set; }
    public bool HasMuseum { get; set; }
    public bool HasDocArchive { get; set; }
    
    public bool HasWasteCollection { get; set; }
    public bool HasWasteWaterSystem { get; set; }
    public bool HasSolidWasteSystem { get; set; }
    public bool HasEmissionTreatment { get; set; }
    public bool HasDedicatedZone { get; set; }
    
    public bool HasSupportPolicy { get; set; }
    public decimal? SupportFunding { get; set; }
    public bool HasTourismActivity { get; set; }
    public bool HasTourLinkage { get; set; }
    public long? AnnualVisitors { get; set; }
    
    public decimal? ExportValueVND { get; private set; } // computed by DB, private set
    public int? IsEffective { get; private set; }       // computed by DB, private set
    
    public DateTime ImportedAt { get; set; }
    public string? DataSource { get; set; }

    // Navigation properties
    public CraftVillage? CraftVillage { get; set; }
    public Region? Commune { get; set; }
    public Region? Province { get; set; }
    public ExportCategory? ExportCategory { get; set; }
    public OutputUnit? OutputUnit { get; set; }
}