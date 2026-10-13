namespace LangNghe.Domain.Enums;

/// <summary>
/// Cấp độ của User (DataScope).
/// </summary>
public enum UserLevelType : byte
{
    /// <summary>
    /// Cấp Trung ương (Bộ NN&PTNT) - Xem toàn quốc
    /// </summary>
    TW = 1,

    /// <summary>
    /// Cấp Tỉnh / Thành phố - Xem và quản lý trong tỉnh
    /// </summary>
    Tinh = 2,

    /// <summary>
    /// Cấp Xã / Phường - Quản lý làng nghề thuộc xã
    /// </summary>
    Xa = 3
}