using System;
using System.Collections.Generic;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Lịch sử lô Import Excel. PK là GUID.
/// Map với bảng Import_History.
/// </summary>
public class ImportHistory
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public string? SheetName { get; set; }
    public int UploadedBy { get; set; }
    public int? UploadedByRegionId { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FileHash { get; set; }
    public string? BlobPath { get; set; }
    public string? TemplateCode { get; set; }
    public string? ImportMode { get; set; }
    public string? ProvidedFieldsJson { get; set; }
    public string? MissingRequiredJson { get; set; }
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int ErrorRows { get; set; }
    public int WarningRows { get; set; }
    public byte Status { get; set; }
    public string? ErrorSummary { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User? UploadedByUser { get; set; }
    public Region? UploadedByRegion { get; set; }
    public ICollection<RawImportStaging> RawImportStagings { get; set; } = new List<RawImportStaging>();
    public ICollection<ImportErrorLog> ImportErrorLogs { get; set; } = new List<ImportErrorLog>();
}