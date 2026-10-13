namespace LangNghe.API.Controllers;

using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.AuditLogs.Commands;
using LangNghe.Application.Features.AuditLogs.Dtos;
using LangNghe.Application.Features.AuditLogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller quản lý và tra cứu nhật ký kiểm toán hệ thống.
/// </summary>
[Route("api/v1/audit-logs")]
[ApiController]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách nhật ký kiểm toán có phân trang và bộ lọc.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AuditView")]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetListAsync(
        [FromQuery] AuditLogFilterDto filter,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAuditLogListQuery(filter), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết một bản ghi nhật ký kiểm toán theo Id.
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "AuditView")]
    public async Task<ActionResult<ApiResponse<AuditLogDto>>> GetByIdAsync(
        long id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAuditLogByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Dọn dẹp các bản ghi nhật ký kiểm toán cũ hơn số ngày chỉ định (chỉ dành cho cấp TW).
    /// </summary>
    [HttpPost("cleanup")]
    [Authorize(Policy = "AuditView")]
    public async Task<ActionResult<ApiResponse<int>>> CleanupAsync(
        [FromQuery] int retentionDays = 730,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new CleanupAuditLogCommand(retentionDays), ct);
        return Ok(result);
    }
}
