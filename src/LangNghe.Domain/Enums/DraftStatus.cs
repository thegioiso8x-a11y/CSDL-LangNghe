namespace LangNghe.Domain.Enums;

/// <summary>
/// Trạng thái của bản nháp báo cáo làng nghề.
/// </summary>
public enum DraftStatus : byte
{
    /// <summary>
    /// Đang lưu nháp, chưa gửi
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Đã nộp thành công, tạo ra Workflow
    /// </summary>
    Submitted = 1
}