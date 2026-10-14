namespace LangNghe.Application.Features.Notifications.Commands;

using LangNghe.Application.Common;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Command đánh dấu toàn bộ thông báo chưa đọc là đã đọc.
/// </summary>
public record MarkAllAsReadCommand : IRequest<ApiResponse<int>>;

/// <summary>
/// Handler đánh dấu toàn bộ thông báo chưa đọc của người dùng là đã đọc.
/// </summary>
public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, ApiResponse<int>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkAllAsReadCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<int>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        var updated = await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, now), cancellationToken);

        return ApiResponse<int>.Ok(updated);
    }
}
