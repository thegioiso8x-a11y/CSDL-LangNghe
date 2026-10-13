namespace LangNghe.Application.Common;

/// <summary>
/// Tham số phân trang từ request.
/// </summary>
public record PageRequest
{
    private int _pageSize = 20;
    private int _page = 1;

    /// <summary>
    /// Trang hiện tại (mặc định là 1, tối thiểu là 1).
    /// </summary>
    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Số lượng bản ghi trên một trang (mặc định là 20, tối đa là 200).
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > 200 ? 200 : (value < 1 ? 1 : value);
    }
}
