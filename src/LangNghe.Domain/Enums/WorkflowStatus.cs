namespace LangNghe.Domain.Enums;

/// <summary>
/// Trạng thái của quy trình xét duyệt/báo cáo.
/// </summary>
public enum WorkflowStatus : byte
{
    /// <summary>
    /// Đang nháp tại cấp Xã.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Chờ cấp Tỉnh duyệt.
    /// </summary>
    PendingProvince = 1,

    /// <summary>
    /// Cấp Tỉnh đã duyệt.
    /// </summary>
    ApprovedProvince = 2,

    /// <summary>
    /// Trung ương đã công bố.
    /// </summary>
    Published = 3,

    /// <summary>
    /// Bị từ chối/Trả lại.
    /// </summary>
    Rejected = 9
}