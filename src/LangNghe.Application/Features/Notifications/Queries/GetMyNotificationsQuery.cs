namespace LangNghe.Application.Features.Notifications.Queries;

using LangNghe.Application.Common;
using LangNghe.Application.Features.Notifications.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Query lấy danh sách thông báo của người dùng hiện tại.
/// </summary>
public record GetMyNotificationsQuery(NotificationFilterDto Filter) : IRequest<ApiResponse<PagedResult<NotificationDto>>>;

/// <summary>
/// Handler lấy danh sách thông báo của người dùng hiện tại.
/// </summary>
public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, ApiResponse<PagedResult<NotificationDto>>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsQueryHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? 0;
        var query = _context.Notifications.AsNoTracking().Where(x => x.UserId == userId);

        if (request.Filter.IsRead.HasValue)
            query = query.Where(x => x.IsRead == request.Filter.IsRead.Value);
        if (!string.IsNullOrEmpty(request.Filter.Type))
            query = query.Where(x => x.Type == request.Filter.Type);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(x => new NotificationDto(x.Id, x.Title, x.Message, x.Type, x.EntityType, x.EntityId, x.LinkUrl, x.IsRead, x.ReadAt, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<PagedResult<NotificationDto>>.Ok(new PagedResult<NotificationDto>(items, request.Filter.Page, request.Filter.PageSize, total));
    }
}
