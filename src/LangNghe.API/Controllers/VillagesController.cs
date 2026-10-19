using LangNghe.Application.Common;
using LangNghe.Application.Features.Village.Commands;
using LangNghe.Application.Features.Village.Dtos;
using LangNghe.Application.Features.Village.Queries;
using LangNghe.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/villages")]
[Authorize]
public class VillagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VillagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] VillageFilterDto filter, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVillageListQuery(filter), ct);
        return Ok(ApiResponse<PagedResult<VillageListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVillageByIdQuery(id), ct);
        return Ok(ApiResponse<VillageDetailDto>.Ok(result));
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVillageByCodeQuery(code), ct);
        return Ok(ApiResponse<VillageDetailDto>.Ok(result));
    }

    [HttpGet("geo")]
    [Authorize(Policy = "VillageRead")]
    public async Task<IActionResult> GetGeoList(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVillageGeoListQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "VillageCreate")]
    public async Task<IActionResult> Create([FromBody] CreateVillageDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateVillageCommand(dto), ct);
        return Ok(ApiResponse<int>.Ok(result, "Tạo mới làng nghề thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "VillageUpdate")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVillageDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateVillageCommand(id, dto), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Cập nhật làng nghề thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "VillageDelete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteVillageCommand(id), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Xóa làng nghề thành công"));
    }

    [HttpPost("{id:int}/publish")]
    [Authorize(Policy = "VillagePublish")]
    public async Task<IActionResult> Publish(int id, [FromQuery] bool publish, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishVillageCommand(id, publish), ct);
        var message = publish ? "Công bố làng nghề thành công" : "Hủy công bố làng nghề thành công";
        return Ok(ApiResponse<bool>.Ok(result, message));
    }

    /// <summary>
    /// Geocode làng nghề từ địa chỉ (dùng Nominatim).
    /// </summary>
    [HttpPost("{id:int}/geocode")]
    [Authorize(Policy = "VillageUpdate")]
    public async Task<IActionResult> Geocode(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GeocodeVillageCommand(id), ct);
        return Ok(result);
    }
}
