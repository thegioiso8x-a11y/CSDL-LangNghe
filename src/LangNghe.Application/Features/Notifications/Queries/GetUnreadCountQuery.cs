namespace LangNghe.Application.Features.Notifications.Queries;

using LangNghe.Application.Common;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Query đếm số thông báo chưa đọc của người dùng hiện tại.
/// </summary>
public record GetUnreadCountQuery : IRequest<ApiResponse<int>>;

/// <summary>
/// Handler đếm thông báo chưa đọc.
/// </summary>
public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, ApiResponse<int>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadCountQueryHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? 0;
        var count = await _context.Notifications
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);

        return ApiResponse<int>.Ok(count);
    }
}
