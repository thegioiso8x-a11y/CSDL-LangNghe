using System.Threading.Tasks;

using LangNghe.Application.Common;
using LangNghe.Application.Features.Workflow.Commands;
using LangNghe.Application.Features.Workflow.Dtos;
using LangNghe.Application.Features.Workflow.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubmissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "VillageRead")]
    public async Task<ActionResult<ApiResponse<PagedResult<SubmissionDto>>>> GetList([FromQuery] SubmissionFilterDto filter)
    {
        return await _mediator.Send(new GetSubmissionListQuery(filter));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "VillageRead")]
    public async Task<ActionResult<ApiResponse<SubmissionDto>>> GetById(long id)
    {
        return await _mediator.Send(new GetSubmissionByIdQuery(id));
    }

    [HttpGet("pending")]
    [Authorize(Policy = "VillageApprove")]
    public async Task<ActionResult<ApiResponse<PagedResult<SubmissionDto>>>> GetPending([FromQuery] PageRequest filter)
    {
        return await _mediator.Send(new GetPendingSubmissionsQuery(filter));
    }

    [HttpPost("submit")]
    [Authorize(Policy = "VillageCreate")]

    public async Task<ActionResult<ApiResponse<long>>> Submit([FromBody] SubmitWorkflowDto dto)
    {
        return await _mediator.Send(new SubmitWorkflowCommand(dto));
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = "VillageApprove")]

    public async Task<ActionResult<ApiResponse<bool>>> Approve(long id)
    {
        return await _mediator.Send(new ApproveWorkflowCommand(id));
    }

    [HttpPost("{id:long}/reject")]
    [Authorize(Policy = "VillageApprove")]

    public async Task<ActionResult<ApiResponse<bool>>> Reject(long id, [FromBody] RejectWorkflowDto dto)
    {
        return await _mediator.Send(new RejectWorkflowCommand(id, dto));
    }

    [HttpPost("{id:long}/publish")]
    [Authorize(Policy = "VillagePublish")]

    public async Task<ActionResult<ApiResponse<bool>>> Publish(long id)
    {
        return await _mediator.Send(new PublishWorkflowCommand(id));
    }
}
