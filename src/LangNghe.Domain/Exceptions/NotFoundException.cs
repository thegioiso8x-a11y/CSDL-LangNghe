namespace LangNghe.Domain.Exceptions;

/// <summary>
/// Bắn ra khi không tìm thấy entity trong database.
/// Trả về HTTP 404 Not Found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Không tìm thấy {name} với định danh ({key}).")
    {
    }
}