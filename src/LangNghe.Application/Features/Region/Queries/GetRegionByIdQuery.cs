using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Region.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Region.Queries;

public record GetRegionByIdQuery(int Id) : IRequest<RegionDto>;

public class GetRegionByIdQueryHandler : IRequestHandler<GetRegionByIdQuery, RegionDto>
{
    private readonly ILangNgheDbContext _context;

    public GetRegionByIdQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<RegionDto> Handle(GetRegionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Regions
            .AsNoTracking()
            .Include(x => x.Parent)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Region), request.Id);

        return new RegionDto
        {
            Id = entity.Id,
            RegionCode = entity.RegionCode,
            Name = entity.Name,
            ShortName = entity.ShortName,
            LevelType = entity.LevelType,
            RegionType = entity.RegionType,
            ParentId = entity.ParentId,
            ParentName = entity.Parent != null ? entity.Parent.Name : null,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
