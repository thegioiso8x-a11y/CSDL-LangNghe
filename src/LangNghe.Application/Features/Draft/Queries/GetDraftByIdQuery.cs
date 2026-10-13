using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LangNghe.Application.Features.Draft.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Draft.Queries;

public record GetDraftByIdQuery(long Id) : IRequest<ApiResponse<DraftDto>>;

public class GetDraftByIdQueryHandler : IRequestHandler<GetDraftByIdQuery, ApiResponse<DraftDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public GetDraftByIdQueryHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DraftDto>> Handle(GetDraftByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.VillageDrafts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("VillageDraft không tồn tại", request.Id);

        return ApiResponse<DraftDto>.Ok(_mapper.Map<DraftDto>(entity));
    }
}
