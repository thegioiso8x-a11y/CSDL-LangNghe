using AutoMapper;
using AutoMapper.QueryableExtensions;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Classification.Dtos;
using LangNghe.Domain.Interfaces;
using LangNghe.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Classification.Queries;

public record GetClassificationByIdQuery(int Id) : IRequest<ClassificationDto>;

public class GetClassificationByIdQueryHandler : IRequestHandler<GetClassificationByIdQuery, ClassificationDto>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public GetClassificationByIdQueryHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ClassificationDto> Handle(GetClassificationByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Classifications
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<ClassificationDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Classification), request.Id);

        return entity;
    }
}
