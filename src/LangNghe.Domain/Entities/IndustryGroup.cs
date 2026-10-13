namespace LangNghe.Domain.Entities;

/// <summary>
/// Nhóm ngành nghề.
/// Map với bảng Dim_IndustryGroups.
/// </summary>
public class IndustryGroup
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<CraftVillage> CraftVillages { get; set; } = new List<CraftVillage>();
    public ICollection<IndustryGroupAggregate> IndustryGroupAggregates { get; set; } = new List<IndustryGroupAggregate>();
}