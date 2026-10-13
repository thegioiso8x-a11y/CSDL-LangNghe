namespace LangNghe.Domain.Enums;

/// <summary>
/// Trạng thái hoạt động của làng nghề.
/// </summary>
public enum VillageStatus : byte
{
    /// <summary>
    /// Ngừng hoạt động
    /// </summary>
    Inactive = 0,

    /// <summary>
    /// Đang hoạt động
    /// </summary>
    Active = 1,

    /// <summary>
    /// Lưu trữ/Giải thể
    /// </summary>
    Archived = 2
}