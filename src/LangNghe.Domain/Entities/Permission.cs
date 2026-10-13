using System.Collections.Generic;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Chức năng/Quyền hạn hệ thống.
/// Map với bảng Sys_Permissions.
/// </summary>
public class Permission
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Module { get; set; }
    public required string Action { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}