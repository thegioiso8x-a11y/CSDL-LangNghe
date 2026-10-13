namespace LangNghe.Domain.Enums;

/// <summary>
/// Phạm vi truy cập dữ liệu của User.
/// </summary>
public enum DataScope : byte
{
    /// <summary>
    /// Trung ương - Xem toàn bộ dữ liệu toàn quốc.
    /// </summary>
    TW = 1,

    /// <summary>
    /// Tỉnh - Xem dữ liệu của tỉnh mình quản lý.
    /// </summary>
    Tinh = 2,

    /// <summary>
    /// Xã/Phường - Chỉ xem và nhập dữ liệu của xã mình.
    /// </summary>
    Xa = 3
}