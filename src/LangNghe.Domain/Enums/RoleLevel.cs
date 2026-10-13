namespace LangNghe.Domain.Enums;

/// <summary>
/// Cấp độ của Role (để giới hạn phân quyền).
/// Khớp với Sys_Roles.Level.
/// </summary>
public enum RoleLevel : byte
{
    /// <summary>
    /// Vai trò Trung ương
    /// </summary>
    TW = 1,

    /// <summary>
    /// Vai trò Tỉnh
    /// </summary>
    Tinh = 2,

    /// <summary>
    /// Vai trò Xã
    /// </summary>
    Xa = 3
}