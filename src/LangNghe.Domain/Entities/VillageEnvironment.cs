using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Hạ tầng và môi trường của làng nghề.
/// Map với bảng Fact_Village_Environment.
/// </summary>
public class VillageEnvironment
{
    public long Id { get; set; }
    public int CraftVillageId { get; set; }
    public required string SurveyPeriod { get; set; }
    public byte? PollutionLevel { get; set; }
    public byte? WasteWaterSystemStatus { get; set; }
    public byte? SolidWasteSystemStatus { get; set; }
    public byte? EmissionSystemStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime ImportedAt { get; set; }

    // Navigation properties
    public CraftVillage? CraftVillage { get; set; }
}