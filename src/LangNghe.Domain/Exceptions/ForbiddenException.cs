namespace LangNghe.Domain.Exceptions;

/// <summary>
/// Bắn ra khi người dùng không có quyền thực hiện hành động.
/// Trả về HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Bạn không có quyền thực hiện thao tác này.")
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }
}