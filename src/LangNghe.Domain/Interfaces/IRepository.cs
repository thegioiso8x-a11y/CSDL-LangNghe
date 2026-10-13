using System.Linq.Expressions;

namespace LangNghe.Domain.Interfaces;

/// <summary>
/// Generic repository pattern interface cho các thực thể.
/// Hạn chế query phức tạp trong repository, chỉ chứa các hàm cơ bản CUD và GetById.
/// Các truy vấn phức tạp nên dùng AutoMapper Projections ở tầng Application hoặc custom Queries.
/// </summary>
/// <typeparam name="T">Loại entity</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Lấy IQueryable để có thể filter thêm bằng LINQ, có hỗ trợ OData hoặc EF Core.
    /// Khuyến nghị: Thêm AsNoTracking() nếu chỉ đọc.
    /// </summary>
    IQueryable<T> AsQueryable();

    /// <summary>
    /// Tìm thực thể theo khóa chính (hỗ trợ composite key).
    /// </summary>
    Task<T?> FindAsync(object[] keyValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm 1 thực thể đầu tiên khớp với điều kiện predicate.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm 1 thực thể vào context.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật 1 thực thể (Thay đổi State thành Modified).
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Xóa 1 thực thể khỏi context (Hard Delete nếu cấu hình, hoặc Soft Delete qua Interceptor).
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Xóa nhiều thực thể.
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);
}