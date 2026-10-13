namespace LangNghe.Application.Features.AuditLogs.Queries;

using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.AuditLogs.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Query lấy chi tiết một bản ghi nhật ký kiểm toán theo Id.
/// </summary>
public record GetAuditLogByIdQuery(long Id) : IRequest<ApiResponse<AuditLogDto>>;

/// <summary>
/// Handler xử lý lấy chi tiết một bản ghi nhật ký kiểm toán.
/// </summary>
public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, ApiResponse<AuditLogDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuditLogByIdQueryHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<AuditLogDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (log == null)
        {
            throw new NotFoundException("AuditLog", request.Id);
        }

        // Kiểm tra quyền DataScope: Người dùng cấp dưới chỉ được xem nhật ký của chính mình
        if (_currentUser.DataScope != 1 && log.UserId != _currentUser.UserId)
        {
            throw new ForbiddenException("Bạn không có quyền xem nhật ký này.");
        }

        var dto = new AuditLogDto(
            log.Id,
            log.UserId,
            log.Username,
            log.Action,
            log.EntityName,
            log.EntityId,
            log.OldValues,
            log.NewValues,
            log.IpAddress,
            log.UserAgent,
            log.CreatedAt);

        return ApiResponse<AuditLogDto>.Ok(dto);
    }
}
