using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Product.Commands;
using LangNghe.Application.Features.Product.Dtos;
using LangNghe.Application.Features.Product.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ProductFilterDto filter, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductListQuery(filter), ct);
        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "VillageCreate")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateProductCommand(dto), ct);
        return Ok(ApiResponse<int>.Ok(result, "Tạo thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "VillageUpdate")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductCommand(id, dto), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Cập nhật thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "VillageDelete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Xóa thành công"));
    }
}
