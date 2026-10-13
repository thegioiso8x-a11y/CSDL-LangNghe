namespace LangNghe.Application.Features.AuditLogs.Commands;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Command dọn dẹp các bản ghi nhật ký kiểm toán cũ hơn số ngày lưu giữ quy định.
/// </summary>
public record CleanupAuditLogCommand(int RetentionDays = 730) : IRequest<ApiResponse<int>>;

/// <summary>
/// Handler xử lý dọn dẹp nhật ký kiểm toán cũ bằng ExecuteDeleteAsync.
/// </summary>
public class CleanupAuditLogCommandHandler : IRequestHandler<CleanupAuditLogCommand, ApiResponse<int>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CleanupAuditLogCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<int>> Handle(CleanupAuditLogCommand request, CancellationToken cancellationToken)
    {
        // Chỉ cấp Trung ương (DataScope = 1) mới có quyền dọn dẹp nhật ký hệ thống
        if (_currentUser.DataScope != 1)
        {
            throw new ForbiddenException("Bạn không có quyền thực hiện thao tác dọn dẹp nhật ký hệ thống.");
        }

        if (request.RetentionDays < 1)
            throw new BusinessException("Số ngày retention tối thiểu là 1.");
        var retentionDays = request.RetentionDays;
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        var deletedCount = await _context.AuditLogs
            .Where(x => x.CreatedAt < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);

        return ApiResponse<int>.Ok(deletedCount, $"Đã xóa thành công {deletedCount} bản ghi nhật ký kiểm toán cũ.");
    }
}
