namespace LangNghe.Domain.Exceptions;

/// <summary>
/// Lỗi kiểm tra tính hợp lệ của dữ liệu đầu vào (FluentValidation).
/// Thường trả về HTTP 400 Bad Request với format đặc biệt.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
        : base("Có một hoặc nhiều lỗi xảy ra khi kiểm tra dữ liệu đầu vào.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : this()
    {
        Errors = errors;
    }

    /// <summary>
    /// Danh sách các lỗi validation được nhóm theo Key (Tên trường dữ liệu).
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }
}