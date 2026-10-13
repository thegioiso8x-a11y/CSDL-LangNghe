using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Cấu hình form nhập liệu động cho Frontend.
/// Map với bảng Sys_FormFieldConfig.
/// </summary>
public class FormFieldConfig
{
    public int Id { get; set; }
    public required string FieldName { get; set; }
    public required string DisplayName { get; set; }
    public required string TabName { get; set; }
    public int FieldOrder { get; set; }
    public required string ControlType { get; set; }
    public string? DropdownSource { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? VisibleForScopes { get; set; }
    public string? EditableForScopes { get; set; }
    public bool IsActive { get; set; }
}