namespace LangNghe.Application.Common;

/// <summary>
/// Cấu trúc phản hồi chung cho toàn bộ API.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> Ok(string message)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message
        };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { message }
        };
    }

    public static ApiResponse<T> Fail(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Có lỗi xảy ra",
            Errors = errors
        };
    }
}
