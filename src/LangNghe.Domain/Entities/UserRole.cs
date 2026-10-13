using System;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Bảng trung gian n-n giữa Người dùng và Vai trò.
/// Map với bảng Sys_UserRoles.
/// </summary>
public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public int? AssignedBy { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Role? Role { get; set; }
}