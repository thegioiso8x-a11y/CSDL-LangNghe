using System.Threading.Tasks;

using LangNghe.Application.Common;
using LangNghe.Application.Features.Draft.Commands;
using LangNghe.Application.Features.Draft.Dtos;
using LangNghe.Application.Features.Draft.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]

public class DraftsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DraftsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "VillageRead")]
    public async Task<ActionResult<ApiResponse<PagedResult<DraftDto>>>> GetList([FromQuery] DraftFilterDto filter)
    {
        return await _mediator.Send(new GetDraftsByUserQuery(filter));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "VillageRead")]
    public async Task<ActionResult<ApiResponse<DraftDto>>> GetById(long id)
    {
        return await _mediator.Send(new GetDraftByIdQuery(id));
    }

    [HttpPost]
    [Authorize(Policy = "VillageCreate")]
    public async Task<ActionResult<ApiResponse<long>>> SaveDraft([FromBody] SaveDraftDto dto)
    {
        return await _mediator.Send(new SaveDraftCommand(dto));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "VillageUpdate")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDraft(long id)
    {
        return await _mediator.Send(new DeleteDraftCommand(id));
    }

    [HttpPost("{id:long}/submit")]
    [Authorize(Policy = "VillageCreate")]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitDraft(long id)
    {
        return await _mediator.Send(new SubmitDraftCommand(id));
    }
}
