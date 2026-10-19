using LangNghe.Application.Features.Village.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Village.Queries;

public record GetVillageByCodeQuery(string Code) : IRequest<VillageDetailDto>;

public class GetVillageByCodeQueryHandler : IRequestHandler<GetVillageByCodeQuery, VillageDetailDto>
{
    private readonly ILangNgheDbContext _context;

    public GetVillageByCodeQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<VillageDetailDto> Handle(GetVillageByCodeQuery request, CancellationToken cancellationToken)
    {
        var village = await _context.CraftVillages.AsNoTracking()
            .Where(v => v.VillageCode == request.Code)
            .Select(v => new VillageDetailDto(
                v.Id,
                v.VillageCode,
                v.Name,
                v.RegionId,
                v.Region != null && v.Region.Parent != null ? v.Region.Parent.Name : null,
                v.Region != null ? v.Region.Name : null,
                v.Address,
                v.ImageUrl,
                v.Latitude,
                v.Longitude,
                v.GeocodedAt,
                v.GeocodeSource,
                v.RecognitionDecision,
                v.RecognitionDate,
                v.ClassificationId,
                v.Classification != null ? v.Classification.Name : null,
                v.VillageTypeId,
                v.VillageType != null ? v.VillageType.Name : null,
                v.IndustryGroupId,
                v.IndustryGroup != null ? v.IndustryGroup.Name : null,
                v.NeedRestoration,
                v.Status,
                v.IsPublished,
                v.PublishedAt,
                v.RowVersion != null ? Convert.ToBase64String(v.RowVersion) : string.Empty,
                v.CreatedAt,
                v.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (village == null)
        {
            throw new NotFoundException("CraftVillage", request.Code);
        }

        return village;
    }
}
