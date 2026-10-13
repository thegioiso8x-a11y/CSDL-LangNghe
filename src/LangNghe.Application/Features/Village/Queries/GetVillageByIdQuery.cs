using LangNghe.Application.Features.Village.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Village.Queries;

public record GetVillageByIdQuery(int Id) : IRequest<VillageDetailDto>;

public class GetVillageByIdQueryHandler : IRequestHandler<GetVillageByIdQuery, VillageDetailDto>
{
    private readonly ILangNgheDbContext _context;

    public GetVillageByIdQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<VillageDetailDto> Handle(GetVillageByIdQuery request, CancellationToken cancellationToken)
    {
        var village = await _context.CraftVillages.AsNoTracking()
            .Where(v => v.Id == request.Id)
            .Select(v => new VillageDetailDto(
                v.Id,
                v.VillageCode,
                v.Name,
                v.RegionId,
                v.Region != null && v.Region.Parent != null ? v.Region.Parent.Name : null,
                v.Region != null ? v.Region.Name : null,
                v.Address,
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
            throw new NotFoundException("CraftVillage", request.Id);
        }

        return village;
    }
}
