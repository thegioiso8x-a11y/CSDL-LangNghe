namespace LangNghe.Domain.Entities;

/// <summary>
/// Phân loại (Làng nghề / Nghề).
/// Map với bảng Dim_Classifications.
/// </summary>
public class Classification
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int? SortOrder { get; set; }

    // Navigation properties
    public ICollection<CraftVillage> CraftVillages { get; set; } = new List<CraftVillage>();
}