namespace LangNghe.Domain.Interfaces;

/// <summary>
/// Interface cho Unit of Work pattern, đảm bảo transaction consistency 
/// khi thay đổi nhiều entity trước khi lưu vào database.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Commit toàn bộ các thay đổi trong context hiện tại xuống database.
    /// </summary>
    /// <returns>Số records bị ảnh hưởng.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}