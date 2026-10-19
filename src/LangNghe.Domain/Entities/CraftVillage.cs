using System;
using System.Collections.Generic;
using LangNghe.Domain.Entities.Common;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Làng nghề — entity dữ liệu cốt lõi do cấp Xã quản lý.
/// Map với bảng Dim_CraftVillages.
/// </summary>
public class CraftVillage : BaseEntity
{
    public required string VillageCode { get; set; }
    public required string Name { get; set; }
    public int RegionId { get; set; }
    public string? Address { get; set; }
    public string? ImageUrl { get; set; }
    /// <summary>Tọa độ vĩ độ (WGS84). Null nếu chưa xác định.</summary>
    public decimal? Latitude { get; set; }

    /// <summary>Tọa độ kinh độ (WGS84). Null nếu chưa xác định.</summary>
    public decimal? Longitude { get; set; }

    /// <summary>Thời điểm tọa độ được xác định (nhập tay hoặc geocode).</summary>
    public DateTime? GeocodedAt { get; set; }

    /// <summary>Nguồn tọa độ: Manual / Nominatim / Import.</summary>
    public string? GeocodeSource { get; set; }
    public string? RecognitionDecision { get; set; }
    public DateTime? RecognitionDate { get; set; }
    public int? ClassificationId { get; set; }
    public int? VillageTypeId { get; set; }
    public int? IndustryGroupId { get; set; }
    public bool NeedRestoration { get; set; }
    public byte Status { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public Region? Region { get; set; }
    public Classification? Classification { get; set; }
    public VillageType? VillageType { get; set; }
    public IndustryGroup? IndustryGroup { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Artisan> Artisans { get; set; } = new List<Artisan>();
    public ICollection<VillagePeriodicMetric> VillagePeriodicMetrics { get; set; } = new List<VillagePeriodicMetric>();
    public ICollection<ProductOcopHistory> ProductOcopHistories { get; set; } = new List<ProductOcopHistory>();
    public ICollection<VillageEnvironment> VillageEnvironments { get; set; } = new List<VillageEnvironment>();
    public ICollection<SubmissionWorkflow> SubmissionWorkflows { get; set; } = new List<SubmissionWorkflow>();
    public ICollection<VillageDraft> VillageDrafts { get; set; } = new List<VillageDraft>();
}
