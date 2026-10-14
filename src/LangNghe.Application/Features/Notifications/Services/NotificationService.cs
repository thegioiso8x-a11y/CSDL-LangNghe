namespace LangNghe.Application.Features.Notifications.Services;

using LangNghe.Application.Features.Notifications.Interfaces;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implementation tạo notification — thêm vào context, KHÔNG tự SaveChanges.
/// Handler gọi service này sẽ save cùng transaction với nghiệp vụ chính.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILangNgheDbContext _context;

    public NotificationService(ILangNgheDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(int userId, string title, string? message, string type,
        string? entityType = null, string? entityId = null, string? linkUrl = null,
        CancellationToken ct = default)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            EntityType = entityType,
            EntityId = entityId,
            LinkUrl = linkUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    public Task CreateBulkAsync(IEnumerable<int> userIds, string title, string? message, string type,
        string? entityType = null, string? entityId = null, string? linkUrl = null,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        foreach (var userId in userIds.Distinct())
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                EntityType = entityType,
                EntityId = entityId,
                LinkUrl = linkUrl,
                IsRead = false,
                CreatedAt = now
            });
        }
        return Task.CompletedTask;
    }

    public Task<int> CountUnreadAsync(int userId, CancellationToken ct = default)
    {
        return _context.Notifications.AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsRead, ct);
    }
}
