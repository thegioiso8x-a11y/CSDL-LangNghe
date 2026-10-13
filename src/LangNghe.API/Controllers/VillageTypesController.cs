using LangNghe.Application.Common;
using LangNghe.Application.Features.VillageType.Commands;
using LangNghe.Application.Features.VillageType.Dtos;
using LangNghe.Application.Features.VillageType.Queries;
using LangNghe.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/village-types")]
[Authorize]
public class VillageTypesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VillageTypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] VillageTypeFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVillageTypeListQuery(filter), cancellationToken);
        return Ok(ApiResponse<PagedResult<VillageTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVillageTypeByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<VillageTypeDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Create([FromBody] CreateVillageTypeDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateVillageTypeCommand(dto), cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Tạo thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVillageTypeDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateVillageTypeCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteVillageTypeCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa thành công"));
    }
}
