using System;
using System.Collections.Generic;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Vai trò người dùng (TW, Tỉnh, Xã, v.v.).
/// Map với bảng Sys_Roles.
/// </summary>
public class Role
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public byte Level { get; set; }
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}