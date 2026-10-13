using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Region.Commands;
using LangNghe.Application.Features.Region.Dtos;
using LangNghe.Application.Features.Region.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/regions")]
[Authorize]
public class RegionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] RegionFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRegionListQuery(filter), cancellationToken);
        return Ok(ApiResponse<PagedResult<RegionDto>>.Ok(result));
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRegionTreeQuery(), cancellationToken);
        return Ok(ApiResponse<List<RegionTreeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRegionByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<RegionDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Create([FromBody] CreateRegionDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateRegionCommand(dto), cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Tạo thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRegionDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateRegionCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteRegionCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa thành công"));
    }
}
