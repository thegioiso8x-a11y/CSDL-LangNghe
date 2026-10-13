namespace LangNghe.Domain.Enums;

/// <summary>
/// Loại kỳ báo cáo.
/// </summary>
public enum PeriodType : byte
{
    /// <summary>
    /// Báo cáo Năm
    /// </summary>
    Yearly = 1,

    /// <summary>
    /// Báo cáo Bán niên (6 tháng)
    /// </summary>
    HalfYear = 2,

    /// <summary>
    /// Báo cáo Quý
    /// </summary>
    Quarterly = 3,

    /// <summary>
    /// Báo cáo Tháng
    /// </summary>
    Monthly = 4
}