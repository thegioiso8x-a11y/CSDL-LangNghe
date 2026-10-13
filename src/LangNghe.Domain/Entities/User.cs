using System;
using System.Collections.Generic;
using LangNghe.Domain.Entities.Common;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Người dùng hệ thống.
/// Map với bảng Sys_Users.
/// </summary>
public class User : BaseEntity
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? RegionId { get; set; }
    public byte DataScope { get; set; }
    public byte? LevelType { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LockedUntil { get; set; }
    public int FailedLoginCount { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public Region? Region { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<SubmissionWorkflow> SubmissionWorkflowsXa { get; set; } = new List<SubmissionWorkflow>();
    public ICollection<SubmissionWorkflow> SubmissionWorkflowsTinh { get; set; } = new List<SubmissionWorkflow>();
    public ICollection<SubmissionWorkflow> SubmissionWorkflowsTW { get; set; } = new List<SubmissionWorkflow>();
    public ICollection<SubmissionWorkflow> SubmissionWorkflowsReject { get; set; } = new List<SubmissionWorkflow>();
    public ICollection<VillageDraft> VillageDrafts { get; set; } = new List<VillageDraft>();
    public ICollection<ImportHistory> ImportHistories { get; set; } = new List<ImportHistory>();
}