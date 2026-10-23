using LangNghe.Application.Common;
using LangNghe.Application.Features.User.Commands;
using LangNghe.Application.Features.User.Dtos;
using LangNghe.Application.Features.User.Queries;
using LangNghe.Application.Features.User.Interfaces;
using LangNghe.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace LangNghe.API.Controllers;

/// <summary>API quản trị người dùng, vai trò và quyền.</summary>
[ApiController]
[Authorize]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserImportService _userImportService;
    public UsersController(IMediator mediator, IUserImportService userImportService) => (_mediator, _userImportService) = (mediator, userImportService);

    [HttpGet, Authorize(Policy = "UserManage")]
    public async Task<IActionResult> GetList([FromQuery] UserFilterDto dto, CancellationToken ct) => Ok(ApiResponse<PagedResult<UserDto>>.Ok(await _mediator.Send(new GetUserListQuery(dto), ct)));

    [HttpGet("{id:int}"), Authorize(Policy = "UserManage")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) => Ok(ApiResponse<UserDto>.Ok(await _mediator.Send(new GetUserByIdQuery(id), ct)));

    [HttpGet("roles"), Authorize(Policy = "UserManage")]
    public async Task<IActionResult> GetRoles(CancellationToken ct) => Ok(ApiResponse<List<RoleDto>>.Ok(await _mediator.Send(new GetRolesQuery(), ct)));

    [HttpGet("permissions"), Authorize(Policy = "UserManage")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct) => Ok(ApiResponse<List<PermissionDto>>.Ok(await _mediator.Send(new GetPermissionsQuery(), ct)));

    [HttpPost, Authorize(Policy = "UserManage")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct) => Ok(ApiResponse<int>.Ok(await _mediator.Send(new CreateUserCommand(dto), ct), "Tạo thành công"));

    [HttpPut("{id:int}"), Authorize(Policy = "UserManage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto, CancellationToken ct) => Ok(ApiResponse<bool>.Ok(await _mediator.Send(new UpdateUserCommand(id, dto), ct), "Cập nhật thành công"));

    [HttpDelete("{id:int}"), Authorize(Policy = "UserManage")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) => Ok(ApiResponse<bool>.Ok(await _mediator.Send(new DeleteUserCommand(id), ct), "Đã vô hiệu hóa người dùng"));

    [HttpPost("{id:int}/reset-password"), Authorize(Policy = "UserManage")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto, CancellationToken ct) => Ok(ApiResponse<bool>.Ok(await _mediator.Send(new ResetPasswordCommand(id, dto), ct), "Đặt lại mật khẩu thành công"));

    [HttpPost("{id:int}/roles"), Authorize(Policy = "UserManage")]
    public async Task<IActionResult> AssignRoles(int id, [FromBody] AssignRolesDto dto, CancellationToken ct) => Ok(ApiResponse<bool>.Ok(await _mediator.Send(new AssignRolesCommand(id, dto), ct), "Gán vai trò thành công"));

    /// <summary>Xem trước file Excel người dùng, chưa ghi dữ liệu.</summary>
    [HttpPost("import/preview"), Authorize(Policy = "UserManage")]
    public async Task<ActionResult<ApiResponse<UserImportPreviewDto>>> PreviewImport([FromForm] UserImportRequest request, CancellationToken ct)
    {
        var file = request.File;
        var importMode = request.ImportMode;

        ValidateImportFile(file);
        using var stream = file.OpenReadStream();
        return Ok(ApiResponse<UserImportPreviewDto>.Ok(await _userImportService.PreviewAsync(stream, importMode, ct)));
    }

    /// <summary>Import chính thức danh sách người dùng từ file Excel.</summary>
    [HttpPost("import"), Authorize(Policy = "UserManage")]
    public async Task<ActionResult<ApiResponse<UserImportResultDto>>> Import([FromForm] UserImportRequest request, CancellationToken ct)
    {
        var file = request.File;
        var importMode = request.ImportMode;
        var skipErrors = request.SkipErrors;

        ValidateImportFile(file);
        using var stream = file.OpenReadStream();
        var result = await _userImportService.ImportAsync(stream, file.FileName, file.Length, importMode, skipErrors, ct);
        return Ok(ApiResponse<UserImportResultDto>.Ok(result, "Import người dùng thành công"));
    }

    /// <summary>Tải file Excel mẫu import người dùng.</summary>
    [HttpGet("import/template"), Authorize(Policy = "UserManage")]
    public IActionResult DownloadImportTemplate() => File(_userImportService.CreateTemplate(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "user-import-template.xlsx");

    private static void ValidateImportFile(IFormFile file)
    {
        if (file == null || file.Length == 0) throw new BusinessException("File không được trống");
        if (file.Length > 20 * 1024 * 1024) throw new BusinessException("Kích thước file không được vượt quá 20MB");
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase)) throw new BusinessException("Chỉ hỗ trợ file Excel .xlsx");
    }
}