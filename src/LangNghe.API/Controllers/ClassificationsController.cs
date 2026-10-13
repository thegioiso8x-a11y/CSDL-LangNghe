using LangNghe.Application.Common;
using LangNghe.Application.Features.Classification.Commands;
using LangNghe.Application.Features.Classification.Dtos;
using LangNghe.Application.Features.Classification.Queries;
using LangNghe.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/classifications")]
[Authorize]
public class ClassificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClassificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ClassificationFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClassificationListQuery(filter), cancellationToken);
        return Ok(ApiResponse<PagedResult<ClassificationDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClassificationByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<ClassificationDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Create([FromBody] CreateClassificationDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateClassificationCommand(dto), cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Tạo thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassificationDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateClassificationCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteClassificationCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa thành công"));
    }
}
