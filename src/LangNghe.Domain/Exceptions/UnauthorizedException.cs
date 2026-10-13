namespace LangNghe.Domain.Exceptions;

/// <summary>
/// Bắn ra khi người dùng chưa đăng nhập hoặc token hết hạn.
/// Trả về HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn.")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }
}