using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Lưu nháp form nhập liệu của cấp xã.
/// Map với bảng Fact_Village_Draft.
/// </summary>
public class VillageDraft
{
    public long Id { get; set; }
    public int? CraftVillageId { get; set; }
    public string? DraftVillageCode { get; set; }
    public required string DraftData { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? AutoSavedAt { get; set; }
    public byte DraftStatus { get; set; }
    public DateTime? SubmittedAt { get; set; }

    // Navigation properties
    public CraftVillage? CraftVillage { get; set; }
    public User? CreatedByUser { get; set; }
}