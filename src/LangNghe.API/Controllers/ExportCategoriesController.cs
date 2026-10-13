using LangNghe.Application.Common;
using LangNghe.Application.Features.ExportCategory.Commands;
using LangNghe.Application.Features.ExportCategory.Dtos;
using LangNghe.Application.Features.ExportCategory.Queries;
using LangNghe.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/export-categories")]
[Authorize]
public class ExportCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExportCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ExportCategoryFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExportCategoryListQuery(filter), cancellationToken);
        return Ok(ApiResponse<PagedResult<ExportCategoryDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExportCategoryByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<ExportCategoryDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Create([FromBody] CreateExportCategoryDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateExportCategoryCommand(dto), cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Tạo thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExportCategoryDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateExportCategoryCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DimManage")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteExportCategoryCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa thành công"));
    }
}
