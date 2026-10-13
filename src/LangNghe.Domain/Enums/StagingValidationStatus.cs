namespace LangNghe.Domain.Enums;

/// <summary>
/// Trạng thái xác thực dữ liệu thô khi Import từ Excel.
/// Khớp với Staging_RawImport.ValidationStatus IN (0,1,2,3).
/// </summary>
public enum StagingValidationStatus : byte
{
    /// <summary>
    /// Chưa xác thực
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Dữ liệu hợp lệ, sẵn sàng import
    /// </summary>
    Valid = 1,

    /// <summary>
    /// Có lỗi dữ liệu (lỗi cấu trúc hoặc logic)
    /// </summary>
    Invalid = 2,

    /// <summary>
    /// Cảnh báo dữ liệu (vẫn có thể import)
    /// </summary>
    Warning = 3
}