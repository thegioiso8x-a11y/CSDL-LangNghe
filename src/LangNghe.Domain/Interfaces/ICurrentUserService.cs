namespace LangNghe.Domain.Interfaces;

/// <summary>
/// Interface cung cấp thông tin về User đang đăng nhập hiện tại.
/// Có thể inject ở bất cứ tầng nào (Domain, Application, Infrastructure).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Khóa chính (Id) của người dùng. Trả về null nếu chưa đăng nhập.
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Tên đăng nhập.
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Cấp bậc phân quyền: 1 (TW), 2 (Tỉnh), 3 (Xã).
    /// </summary>
    int? DataScope { get; }

    /// <summary>
    /// Mã vùng tương ứng với user. Có thể là ID Tỉnh hoặc ID Xã.
    /// Nếu là TW (DataScope = 1) thì RegionId thường null.
    /// </summary>
    int? RegionId { get; }

    /// <summary>
    /// Phân cấp vùng (1: Tỉnh, 2: Xã) - dùng cho các filter nhanh.
    /// </summary>
    int? LevelType { get; }

    /// <summary>
    /// Danh sách tất cả Permission Codes mà User đang có.
    /// </summary>
    IEnumerable<string> Permissions { get; }
}