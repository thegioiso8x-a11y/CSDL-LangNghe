using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LangNghe.Application.Features.Workflow.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Workflow.Queries;

public record GetSubmissionByIdQuery(long Id) : IRequest<ApiResponse<SubmissionDto>>;

public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, ApiResponse<SubmissionDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public GetSubmissionByIdQueryHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SubmissionDto>> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.SubmissionWorkflows
            .AsNoTracking()
            .Include(x => x.CraftVillage)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("SubmissionWorkflow không tồn tại", request.Id);

        var dto = _mapper.Map<SubmissionDto>(entity);
        return ApiResponse<SubmissionDto>.Ok(dto);
    }
}
