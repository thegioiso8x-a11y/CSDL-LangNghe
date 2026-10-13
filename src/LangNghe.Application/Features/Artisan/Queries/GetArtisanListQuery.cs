using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Artisan.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Artisan.Queries;

public record GetArtisanListQuery(ArtisanFilterDto Dto) : IRequest<PagedResult<ArtisanDto>>;

public class GetArtisanListQueryHandler : IRequestHandler<GetArtisanListQuery, PagedResult<ArtisanDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetArtisanListQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArtisanDto>> Handle(GetArtisanListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Artisans
            .AsNoTracking()
            .Include(x => x.CraftVillage)
            .AsQueryable();

        if (request.Dto.CraftVillageId.HasValue)
        {
            query = query.Where(x => x.CraftVillageId == request.Dto.CraftVillageId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.Keyword))
        {
            var keyword = request.Dto.Keyword.Trim();
            query = query.Where(x => x.FullName.Contains(keyword) || (x.Phone != null && x.Phone.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.ArtisanLevel))
        {
            var level = request.Dto.ArtisanLevel.Trim();
            query = query.Where(x => x.ArtisanLevel == level);
        }

        if (request.Dto.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.Dto.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.FullName)
            .Skip((request.Dto.Page - 1) * request.Dto.PageSize)
            .Take(request.Dto.PageSize)
            .Select(x => new ArtisanDto(
                x.Id,
                x.CraftVillageId,
                x.CraftVillage != null ? x.CraftVillage.Name : null,
                x.FullName,
                x.BirthYear,
                x.Gender,
                x.ArtisanLevel,
                x.RecognitionYear,
                x.Specialty,
                x.Phone,
                x.IsActive,
                x.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ArtisanDto>(items, request.Dto.Page, request.Dto.PageSize, totalCount);
    }
}
