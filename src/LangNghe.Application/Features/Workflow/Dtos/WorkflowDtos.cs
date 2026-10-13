using LangNghe.Application.Common;
using System;

namespace LangNghe.Application.Features.Workflow.Dtos;

public record SubmissionDto
{
    public long Id { get; init; }
    public int CraftVillageId { get; init; }
    public string VillageName { get; init; } = default!;
    public string ReportPeriod { get; init; } = default!;
    public byte CurrentStatus { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SentToProvinceAt { get; init; }
    public DateTime? ApprovedByTinhAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? RejectedAt { get; init; }
    public string? RejectionReason { get; init; }
    public int Version { get; init; }
}

public record SubmissionFilterDto : PageRequest
{
    public int? CraftVillageId { get; set; }
    public byte? Status { get; set; }
    public string? ReportPeriod { get; set; }
}

public record RejectWorkflowDto(string Reason);

public record SubmitWorkflowDto(int CraftVillageId, string ReportPeriod, string? Notes);
