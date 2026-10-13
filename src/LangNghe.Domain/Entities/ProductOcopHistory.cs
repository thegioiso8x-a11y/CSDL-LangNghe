using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Lịch sử đánh giá sao OCOP của sản phẩm.
/// Map với bảng Fact_Product_OCOP_History.
/// </summary>
public class ProductOcopHistory
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public int CraftVillageId { get; set; }
    public int EvaluatedYear { get; set; }
    public byte OcopStar { get; set; }
    public string? YieldCapacity { get; set; }
    public string? MainMarket { get; set; }
    public string? EvaluatedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime ImportedAt { get; set; }

    // Navigation properties
    public Product? Product { get; set; }
    public CraftVillage? CraftVillage { get; set; }
}