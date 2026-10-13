using LangNghe.Application.Common;
using LangNghe.Application.Features.OutputUnit.Commands;
using LangNghe.Application.Features.OutputUnit.Dtos;
using LangNghe.Application.Features.OutputUnit.Queries;
using LangNghe.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/output-units")]
[Authorize]
public class OutputUnitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OutputUnitsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] OutputUnitFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOutputUnitListQuery(filter), cancellationToken);
        return Ok(ApiResponse<PagedResult<OutputUnitDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOutputUnitByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<OutputUnitDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Create([FromBody] CreateOutputUnitDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateOutputUnitCommand(dto), cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Tạo thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOutputUnitDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateOutputUnitCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteOutputUnitCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa thành công"));
    }
}
