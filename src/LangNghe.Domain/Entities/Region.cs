using System;
using System.Collections.Generic;
using LangNghe.Domain.Entities.Common;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Danh mục vùng miền/địa giới hành chính (2 cấp: Tỉnh và Xã/Phường/Đặc khu).
/// Map với bảng Dim_Regions.
/// </summary>
public class Region : BaseEntity
{
    public required string RegionCode { get; set; }
    public required string Name { get; set; }
    public string? ShortName { get; set; }
    public byte LevelType { get; set; }
    public string? RegionType { get; set; }
    public int? ParentId { get; set; }
    public string? MergedFromCode { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Region? Parent { get; set; }
    public ICollection<Region> Children { get; set; } = new List<Region>();
    public ICollection<CraftVillage> CraftVillages { get; set; } = new List<CraftVillage>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ImportHistory> ImportHistories { get; set; } = new List<ImportHistory>();
    public ICollection<VillageTimeSeries> VillageTimeSeries { get; set; } = new List<VillageTimeSeries>();
}