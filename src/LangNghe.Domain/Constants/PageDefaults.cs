namespace LangNghe.Domain.Constants;

/// <summary>
/// Các hằng số mặc định cho phân trang.
/// </summary>
public static class PageDefaults
{
    /// <summary>
    /// Số lượng bản ghi mặc định hiển thị trên 1 trang.
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Số lượng bản ghi tối đa được phép lấy trong 1 request.
    /// </summary>
    public const int MaxPageSize = 200;

    /// <summary>
    /// Trang bắt đầu.
    /// </summary>
    public const int DefaultPageIndex = 1;
}