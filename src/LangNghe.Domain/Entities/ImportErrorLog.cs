using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Lỗi chi tiết phát sinh trong quá trình Import.
/// Map với bảng Import_Error_Log.
/// </summary>
public class ImportErrorLog
{
    public long Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public int RowNumber { get; set; }
    public string? ColumnName { get; set; }
    public string? ErrorCode { get; set; }
    public required string ErrorMessage { get; set; }
    public string? RawValue { get; set; }
    public byte Severity { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ImportHistory? ImportHistory { get; set; }
}