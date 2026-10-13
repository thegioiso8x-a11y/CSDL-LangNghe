namespace LangNghe.Domain.Interfaces;

/// <summary>
/// Interface cho các Entity dùng cơ chế xóa mềm (Soft Delete) thông qua cờ IsActive.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Đánh dấu bản ghi còn hoạt động hay không. True = Đang hoạt động, False = Đã xóa mềm.
    /// </summary>
    bool IsActive { get; set; }
}