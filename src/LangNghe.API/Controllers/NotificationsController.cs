namespace LangNghe.API.Controllers;

using LangNghe.Application.Common;
using LangNghe.Application.Features.Notifications.Commands;
using LangNghe.Application.Features.Notifications.Dtos;
using LangNghe.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API quản lý thông báo in-app của người dùng hiện tại.
/// </summary>
[Route("api/v1/notifications")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách thông báo của người dùng hiện tại.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetMyNotificationsAsync(
        [FromQuery] NotificationFilterDto filter,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery(filter), ct);
        return Ok(result);
    }

    /// <summary>
    /// Đếm số thông báo chưa đọc.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCountAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUnreadCountQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Đánh dấu một thông báo là đã đọc.
    /// </summary>
    [HttpPost("{id:long}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsReadAsync(long id, CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Đánh dấu toàn bộ thông báo là đã đọc.
    /// </summary>
    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllAsReadAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkAllAsReadCommand(), ct);
        return Ok(result);
    }
}
