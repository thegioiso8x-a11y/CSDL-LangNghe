using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Quy trình trình duyệt 2 bước của báo cáo định kỳ.
/// Map với bảng Fact_SubmissionWorkflow.
/// </summary>
public class SubmissionWorkflow
{
    public long Id { get; set; }
    public int CraftVillageId { get; set; }
    public required string ReportPeriod { get; set; }
    public byte CurrentStatus { get; set; }
    public int CreatedByXa { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentToProvinceAt { get; set; }
    public int? ApprovedByTinh { get; set; }
    public DateTime? ApprovedByTinhAt { get; set; }
    public int? PublishedByTW { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int Version { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public CraftVillage? CraftVillage { get; set; }
    public User? UserXa { get; set; }
    public User? UserTinh { get; set; }
    public User? UserTW { get; set; }
    public User? UserReject { get; set; }
}