namespace LangNghe.Domain.Enums;

/// <summary>
/// Trạng thái của một tiến trình import dữ liệu.
/// </summary>
public enum ImportStatus : byte
{
    /// <summary>
    /// Chờ xử lý
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Đang xử lý
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Hoàn thành thành công 100%
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Hoàn thành nhưng có lỗi (<= 10%)
    /// </summary>
    CompletedWithErrors = 4,

    /// <summary>
    /// Thất bại (Rollback do lỗi > 10% hoặc lỗi hệ thống)
    /// </summary>
    Failed = 5
}