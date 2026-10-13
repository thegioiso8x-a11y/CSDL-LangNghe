using LangNghe.Application.Common;
using LangNghe.Application.Features.IndustryGroup.Commands;
using LangNghe.Application.Features.IndustryGroup.Dtos;
using LangNghe.Application.Features.IndustryGroup.Queries;
using LangNghe.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/industry-groups")]
[Authorize]
public class IndustryGroupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IndustryGroupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] IndustryGroupFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetIndustryGroupListQuery(filter), cancellationToken);
        return Ok(ApiResponse<PagedResult<IndustryGroupDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetIndustryGroupByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<IndustryGroupDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Create([FromBody] CreateIndustryGroupDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateIndustryGroupCommand(dto), cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Tạo thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIndustryGroupDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateIndustryGroupCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteIndustryGroupCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa thành công"));
    }
}
