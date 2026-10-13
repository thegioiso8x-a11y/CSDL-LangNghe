using AutoMapper;
using AutoMapper.QueryableExtensions;
using LangNghe.Application.Common;
using LangNghe.Application.Features.VillageType.Dtos;
using LangNghe.Domain.Interfaces;
using LangNghe.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.VillageType.Queries;

public record GetVillageTypeByIdQuery(int Id) : IRequest<VillageTypeDto>;

public class GetVillageTypeByIdQueryHandler : IRequestHandler<GetVillageTypeByIdQuery, VillageTypeDto>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public GetVillageTypeByIdQueryHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VillageTypeDto> Handle(GetVillageTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.VillageTypes
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<VillageTypeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.VillageType), request.Id);

        return entity;
    }
}
