using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Bảng trung gian n-n giữa Vai trò và Quyền hạn.
/// Map với bảng Sys_RolePermissions.
/// </summary>
public class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public DateTime GrantedAt { get; set; }

    // Navigation properties
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}