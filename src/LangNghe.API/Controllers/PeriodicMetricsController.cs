using System.Collections.Generic;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.PeriodicMetric.Commands;
using LangNghe.Application.Features.PeriodicMetric.Dtos;
using LangNghe.Application.Features.PeriodicMetric.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

/// <summary>
/// Quản lý chỉ tiêu định kỳ của làng nghề
/// </summary>
[ApiController]
[Route("api/v1/periodic-metrics")]
[Authorize]
public class PeriodicMetricsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PeriodicMetricsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách chỉ tiêu định kỳ (có phân trang và tìm kiếm)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MetricListDto>>>> GetList([FromQuery] MetricFilterDto filter)
    {
        var result = await _mediator.Send(new GetMetricListQuery(filter));
        return Ok(ApiResponse<PagedResult<MetricListDto>>.Ok(result));
    }

    /// <summary>
    /// Lấy chi tiết chỉ tiêu định kỳ theo Id
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<MetricDto>>> GetById(long id)
    {
        var result = await _mediator.Send(new GetMetricByIdQuery(id));
        return Ok(ApiResponse<MetricDto>.Ok(result));
    }

    /// <summary>
    /// Lấy danh sách chỉ tiêu định kỳ theo làng nghề
    /// </summary>
    [HttpGet("by-village/{villageId:int}")]
    public async Task<ActionResult<ApiResponse<List<MetricDto>>>> GetByVillage(int villageId)
    {
        var result = await _mediator.Send(new GetMetricsByVillageQuery(villageId));
        return Ok(ApiResponse<List<MetricDto>>.Ok(result));
    }

    /// <summary>
    /// Tạo mới chỉ tiêu định kỳ
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "VillageCreate")]
    public async Task<ActionResult<ApiResponse<long>>> Create([FromBody] CreateMetricDto dto)
    {
        var id = await _mediator.Send(new CreateMetricCommand(dto));
        return Ok(ApiResponse<long>.Ok(id));
    }

    /// <summary>
    /// Cập nhật chỉ tiêu định kỳ
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Policy = "VillageUpdate")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(long id, [FromBody] UpdateMetricDto dto)
    {
        var result = await _mediator.Send(new UpdateMetricCommand(id, dto));
        return Ok(ApiResponse<bool>.Ok(result));
    }

    /// <summary>
    /// Xóa chỉ tiêu định kỳ
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = "VillageDelete")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteMetricCommand(id));
        return Ok(ApiResponse<bool>.Ok(result));
    }

    /// <summary>
    /// Sao chép chỉ tiêu từ kỳ báo cáo gần nhất của làng nghề
    /// </summary>
    [HttpPost("copy-previous")]
    [Authorize(Policy = "VillageCreate")]
    public async Task<ActionResult<ApiResponse<long>>> CopyPrevious([FromBody] CopyPreviousMetricCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(ApiResponse<long>.Ok(id));
    }
}
