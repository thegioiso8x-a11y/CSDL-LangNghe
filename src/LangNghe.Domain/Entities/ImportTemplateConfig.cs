using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Cấu hình ánh xạ cột Excel cho từng loại Template Import.
/// Map với bảng Import_TemplateConfig.
/// </summary>
public class ImportTemplateConfig
{
    public int Id { get; set; }
    public required string TemplateCode { get; set; }
    public required string FieldName { get; set; }
    public required string ExcelColumnHeader { get; set; }
    public int FieldOrder { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public required string DataType { get; set; }
    public int? MaxLength { get; set; }
    public string? ValidationRegex { get; set; }
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public string? HelpText { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}