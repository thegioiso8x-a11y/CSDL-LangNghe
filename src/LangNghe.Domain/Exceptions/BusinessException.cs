namespace LangNghe.Domain.Exceptions;

/// <summary>
/// Lỗi nghiệp vụ từ Domain hoặc Application layer.
/// Thường trả về HTTP 400 Bad Request.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }

    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}