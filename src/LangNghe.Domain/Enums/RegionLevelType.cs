namespace LangNghe.Domain.Enums;

/// <summary>
/// Cấp hành chính của Vùng.
/// Khớp với Dim_Regions.LevelType.
/// </summary>
public enum RegionLevelType : byte
{
    /// <summary>
    /// Cấp Tỉnh / Thành phố trực thuộc trung ương
    /// </summary>
    Tinh = 1,

    /// <summary>
    /// Cấp Xã / Phường / Thị trấn
    /// </summary>
    Xa = 2
}