using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Artisan.Commands;
using LangNghe.Application.Features.Artisan.Dtos;
using LangNghe.Application.Features.Artisan.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/artisans")]
public class ArtisansController : ControllerBase
{
    private readonly IMediator _mediator;

    public ArtisansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ArtisanFilterDto filter, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetArtisanListQuery(filter), ct);
        return Ok(ApiResponse<PagedResult<ArtisanDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetArtisanByIdQuery(id), ct);
        return Ok(ApiResponse<ArtisanDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "VillageCreate")]
    public async Task<IActionResult> Create([FromBody] CreateArtisanDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateArtisanCommand(dto), ct);
        return Ok(ApiResponse<int>.Ok(result, "Tạo thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "VillageUpdate")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateArtisanDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateArtisanCommand(id, dto), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Cập nhật thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "VillageDelete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteArtisanCommand(id), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Xóa thành công"));
    }
}
