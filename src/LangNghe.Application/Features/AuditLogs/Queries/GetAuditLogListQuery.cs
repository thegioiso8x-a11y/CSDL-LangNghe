namespace LangNghe.Application.Features.AuditLogs.Queries;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.AuditLogs.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Query lấy danh sách nhật ký kiểm toán có phân trang và bộ lọc.
/// </summary>
public record GetAuditLogListQuery(AuditLogFilterDto Filter) : IRequest<ApiResponse<PagedResult<AuditLogDto>>>;

/// <summary>
/// Handler xử lý truy vấn danh sách nhật ký kiểm toán.
/// </summary>
public class GetAuditLogListQueryHandler : IRequestHandler<GetAuditLogListQuery, ApiResponse<PagedResult<AuditLogDto>>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuditLogListQueryHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResult<AuditLogDto>>> Handle(GetAuditLogListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking();

        // Phân quyền DataScope: Chỉ DataScope = 1 (TW) mới được xem toàn bộ, các cấp khác chỉ xem của chính mình
        if (_currentUser.DataScope != 1)
        {
            query = query.Where(x => x.UserId == _currentUser.UserId);
        }
        else if (request.Filter.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.Filter.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Filter.Action))
        {
            var action = request.Filter.Action.Trim();
            query = query.Where(x => x.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(request.Filter.EntityName))
        {
            var entityName = request.Filter.EntityName.Trim();
            query = query.Where(x => x.EntityName == entityName);
        }

        if (request.Filter.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.Filter.FromDate.Value);
        }

        if (request.Filter.ToDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.Filter.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Filter.Keyword))
        {
            var keyword = request.Filter.Keyword.Trim();
            query = query.Where(x => (x.EntityId != null && x.EntityId.Contains(keyword)) ||
                                     (x.Username != null && x.Username.Contains(keyword)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(x => new AuditLogDto(
                x.Id,
                x.UserId,
                x.Username,
                x.Action,
                x.EntityName,
                x.EntityId,
                x.OldValues,
                x.NewValues,
                x.IpAddress,
                x.UserAgent,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AuditLogDto>(items, request.Filter.Page, request.Filter.PageSize, totalCount);
        return ApiResponse<PagedResult<AuditLogDto>>.Ok(result);
    }
}
