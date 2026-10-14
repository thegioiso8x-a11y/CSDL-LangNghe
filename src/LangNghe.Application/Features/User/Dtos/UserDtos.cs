using LangNghe.Application.Common;

namespace LangNghe.Application.Features.User.Dtos;

/// <summary>Thông tin người dùng trả về cho màn hình quản trị.</summary>
public record UserDto(
    int Id,
    string Username,
    string? FullName,
    string? Email,
    string? Phone,
    int? RegionId,
    string? RegionName,
    byte DataScope,
    string? DataScopeName,
    bool IsActive,
    DateTime? LastLoginAt,
    List<string> Roles,
    DateTime CreatedAt);

/// <summary>Dữ liệu tạo mới người dùng.</summary>
public record CreateUserDto(
    string Username,
    string Password,
    string? FullName,
    string? Email,
    string? Phone,
    int? RegionId,
    byte DataScope,
    bool MustChangePassword,
    List<int> RoleIds);

/// <summary>Dữ liệu cập nhật người dùng.</summary>
public record UpdateUserDto(
    string? FullName,
    string? Email,
    string? Phone,
    int? RegionId,
    byte DataScope,
    bool IsActive,
    List<int> RoleIds);

/// <summary>Bộ lọc danh sách người dùng.</summary>
public record UserFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    public int? RegionId { get; init; }
    public byte? DataScope { get; init; }
    public bool? IsActive { get; init; }
    public string? RoleCode { get; init; }
}

/// <summary>Thông tin vai trò.</summary>
public record RoleDto(int Id, string Code, string Name, string? Description);

/// <summary>Thông tin quyền hạn.</summary>
public record PermissionDto(int Id, string Code, string Module, string Action, string? Name);

/// <summary>Dữ liệu đặt lại mật khẩu.</summary>
public record ResetPasswordDto(string NewPassword);

/// <summary>Dữ liệu gán vai trò cho người dùng.</summary>
public record AssignRolesDto(List<int> RoleIds);

/// <summary>Thông tin xem trước một dòng import người dùng.</summary>
public record UserImportRowDto(
    int RowNumber,
    string Username,
    string? FullName,
    string DataScope,
    string? RegionCode,
    List<string> RoleCodes,
    bool IsValid,
    List<string> Errors);

/// <summary>Kết quả phân tích file import người dùng.</summary>
public record UserImportPreviewDto(
    int TotalRows,
    int ValidRows,
    int ErrorRows,
    List<UserImportRowDto> Rows);

/// <summary>Một lỗi của dòng import người dùng.</summary>
public record UserImportErrorDto(int RowNumber, string Username, string Message);

/// <summary>Kết quả import người dùng vào cơ sở dữ liệu.</summary>
public record UserImportResultDto(
    int TotalRows,
    int ImportedRows,
    int UpdatedRows,
    int SkippedRows,
    int ErrorRows,
    List<UserImportErrorDto> Errors);
