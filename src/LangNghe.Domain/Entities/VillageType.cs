namespace LangNghe.Domain.Entities;

/// <summary>
/// Loại hình làng nghề.
/// Map với bảng Dim_VillageTypes.
/// </summary>
public class VillageType
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool IsTraditional { get; set; }
    public bool IsRestored { get; set; }
    public bool IsNew { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<CraftVillage> CraftVillages { get; set; } = new List<CraftVillage>();
    public ICollection<VillageTypeAggregate> VillageTypeAggregates { get; set; } = new List<VillageTypeAggregate>();
}