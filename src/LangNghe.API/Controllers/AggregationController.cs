namespace LangNghe.API.Controllers;

using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Aggregation.Commands;
using LangNghe.Application.Features.Aggregation.Interfaces;
using LangNghe.Application.Features.Aggregation.Queries;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/aggregation")]
[ApiController]
[Authorize]
public class AggregationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IExportService _exportService;

    public AggregationController(IMediator mediator, ICurrentUserService currentUser, IExportService exportService)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _exportService = exportService;
    }

    /// <summary>
    /// Rebuild thủ công toàn bộ dữ liệu tổng hợp (Dashboard) cho một kỳ báo cáo.
    /// Chỉ user có DataScope = 1 (Trung ương) mới có quyền thực hiện.
    /// </summary>
    [HttpPost("rebuild")]
    [Authorize(Policy = "ReportExport")]
    public async Task<ActionResult<ApiResponse<bool>>> RebuildAggregationAsync(
        [FromQuery] string period, 
        [FromQuery] byte periodType,
        CancellationToken ct)
    {
        if (_currentUser.DataScope != 1)
        {
            throw new ForbiddenException("Bạn không có quyền rebuild dữ liệu tổng hợp.");
        }

        var result = await _mediator.Send(new RebuildAggCommand(period, periodType), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy dữ liệu tổng hợp cho Dashboard theo chu kỳ và vùng.
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Policy = "ReportView")]
    public async Task<ActionResult<ApiResponse<DashboardMetricsDto>>> GetDashboardMetricsAsync(
        [FromQuery] string period,
        [FromQuery] byte periodType,
        [FromQuery] int regionId,
        CancellationToken ct)
    {
        var query = new GetDashboardMetricsQuery
        {
            Period = period,
            PeriodType = periodType,
            RegionId = regionId
        };
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost("export/summary")]
    [Authorize(Policy = "ReportExport")]
    public async Task<IActionResult> ExportSummaryAsync(
        [FromQuery] string period, 
        [FromQuery] byte periodType, 
        [FromQuery] int regionId, 
        [FromQuery] string format = "xlsx",
        CancellationToken ct = default)
    {
        var fileData = await _exportService.ExportSummaryAsync(period, periodType, regionId, ct);
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"bao-cao-tong-{period}.xlsx");
    }

    [HttpPost("export/detail")]
    [Authorize(Policy = "ReportExport")]
    public async Task<IActionResult> ExportDetailAsync(
        [FromQuery] string period, 
        [FromQuery] byte periodType, 
        [FromQuery] int regionId, 
        [FromQuery] string format = "xlsx",
        CancellationToken ct = default)
    {
        var fileData = await _exportService.ExportDetailAsync(period, periodType, regionId, ct);
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"bao-cao-chi-tiet-{period}.xlsx");
    }
}
