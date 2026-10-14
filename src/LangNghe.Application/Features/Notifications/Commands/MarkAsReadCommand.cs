namespace LangNghe.Application.Features.Notifications.Commands;

using LangNghe.Application.Common;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Command đánh dấu một thông báo là đã đọc.
/// </summary>
public record MarkAsReadCommand(long Id) : IRequest<ApiResponse<bool>>;

/// <summary>
/// Handler đánh dấu một thông báo là đã đọc.
/// </summary>
public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, ApiResponse<bool>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkAsReadCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? 0;
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        if (notification is null)
            throw new NotFoundException("Notification", request.Id);

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }
}
