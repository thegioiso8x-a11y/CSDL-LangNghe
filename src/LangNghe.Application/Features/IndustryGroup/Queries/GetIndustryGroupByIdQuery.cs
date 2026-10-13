using AutoMapper;
using AutoMapper.QueryableExtensions;
using LangNghe.Application.Common;
using LangNghe.Application.Features.IndustryGroup.Dtos;
using LangNghe.Domain.Interfaces;
using LangNghe.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.IndustryGroup.Queries;

public record GetIndustryGroupByIdQuery(int Id) : IRequest<IndustryGroupDto>;

public class GetIndustryGroupByIdQueryHandler : IRequestHandler<GetIndustryGroupByIdQuery, IndustryGroupDto>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public GetIndustryGroupByIdQueryHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IndustryGroupDto> Handle(GetIndustryGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.IndustryGroups
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<IndustryGroupDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.IndustryGroup), request.Id);

        return entity;
    }
}
