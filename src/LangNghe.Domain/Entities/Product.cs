using System;
using System.Collections.Generic;
using LangNghe.Domain.Entities.Common;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Sản phẩm tiêu biểu và OCOP của làng nghề.
/// Map với bảng Dim_Products.
/// </summary>
public class Product : BaseEntity
{
    public int CraftVillageId { get; set; }
    public string? ProductCode { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Materials { get; set; }
    public string? ImageUrl { get; set; }
    public bool HasOCOP { get; set; }
    public byte? OcopStar { get; set; }
    public int? OcopEvaluatedYear { get; set; }
    public string? MainMarket { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public CraftVillage? CraftVillage { get; set; }
    public ICollection<ProductOcopHistory> ProductOcopHistories { get; set; } = new List<ProductOcopHistory>();
}