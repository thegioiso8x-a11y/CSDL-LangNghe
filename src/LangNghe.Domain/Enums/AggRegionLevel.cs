namespace LangNghe.Domain.Enums;

/// <summary>
/// Phân cấp dữ liệu báo cáo / tổng hợp.
/// Dùng cho Agg_Village_TimeSeries.RegionLevel.
/// </summary>
public enum AggRegionLevel : byte
{
    /// <summary>
    /// Số liệu tổng hợp Toàn quốc
    /// </summary>
    ToanQuoc = 0,

    /// <summary>
    /// Số liệu cấp Tỉnh
    /// </summary>
    Tinh = 1,

    /// <summary>
    /// Số liệu cấp Xã
    /// </summary>
    Xa = 2
}