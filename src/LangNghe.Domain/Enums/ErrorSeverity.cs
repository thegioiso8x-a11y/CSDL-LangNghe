namespace LangNghe.Domain.Enums;

/// <summary>
/// Mức độ nghiêm trọng của lỗi import.
/// </summary>
public enum ErrorSeverity : byte
{
    /// <summary>
    /// Lỗi nghiêm trọng, không thể lưu dữ liệu.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Cảnh báo, dữ liệu vẫn được lưu nhưng cần chú ý.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Thông tin bổ sung.
    /// </summary>
    Info = 3
}