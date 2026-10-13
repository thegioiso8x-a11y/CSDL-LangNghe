using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Dữ liệu thô lưu tạm thời trong quá trình Import.
/// Map với bảng Staging_RawImport.
/// </summary>
public class RawImportStaging
{
    public long Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public int RowNumber { get; set; }
    public string? SheetName { get; set; }
    public required string RawData { get; set; }
    public string? ParsedVillageCode { get; set; }
    public int? ParsedRegionId { get; set; }
    public byte ValidationStatus { get; set; }
    public string? ValidationErrors { get; set; }
    public int? PersistedEntityId { get; set; }
    public DateTime? PersistedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ImportHistory? ImportHistory { get; set; }
}