using System;
using System.Collections.Generic;
using LangNghe.Application.Common;

namespace LangNghe.Application.Features.Import.Dtos;

/// <summary>
/// Kết quả import
/// </summary>
public record ImportResultDto(
    Guid BatchId, 
    int TotalRows, 
    int SuccessRows, 
    int ErrorRows, 
    int WarningRows, 
    byte Status, 
    string? Message,
    string? FileName = null,
    string? TemplateCode = null,
    string? ImportMode = null,
    DateTime? StartedAt = null,
    DateTime? CompletedAt = null);

/// <summary>
/// Chi tiết dữ liệu từng dòng import (dùng cho việc xem trước/xác thực)
/// </summary>
public record ImportPreviewRowDto(
    int RowNumber, 
    string? VillageCode, 
    string? VillageName, 
    byte ValidationStatus, 
    List<string> Errors);

/// <summary>
/// Dữ liệu tổng quan khi preview bản ghi import
/// </summary>
public record ImportPreviewDto(
    Guid BatchId, 
    int TotalRows, 
    List<ImportPreviewRowDto> Rows);

/// <summary>
/// Bộ lọc tìm kiếm các bản ghi import
/// </summary>
public record ImportFilterDto : PageRequest
{
    public Guid? BatchId { get; set; }
    public byte? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
