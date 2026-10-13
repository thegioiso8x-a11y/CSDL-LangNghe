namespace LangNghe.Application.Common;

/// <summary>
/// Cấu trúc dữ liệu trả về cho danh sách có phân trang.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của phần tử trong danh sách</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Constructor rỗng cho serialization.</summary>
    public PagedResult() { }

    /// <summary>Constructor đầy đủ cho Handler.</summary>
    public PagedResult(List<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
