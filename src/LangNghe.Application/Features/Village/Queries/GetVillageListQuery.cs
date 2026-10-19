using LangNghe.Application.Common;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LangNghe.Application.Features.Village.Dtos;

namespace LangNghe.Application.Features.Village.Queries;

public record GetVillageListQuery(VillageFilterDto Dto) : IRequest<PagedResult<VillageListDto>>;

public class GetVillageListQueryHandler : IRequestHandler<GetVillageListQuery, PagedResult<VillageListDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetVillageListQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<VillageListDto>> Handle(GetVillageListQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Dto;
        var query = _context.CraftVillages.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            query = query.Where(v => v.Name.Contains(filter.Keyword) || v.VillageCode.Contains(filter.Keyword));
        }

        if (filter.ProvinceId.HasValue)
        {
            query = query.Where(v => v.Region != null && v.Region.ParentId == filter.ProvinceId);
        }

        if (filter.CommuneId.HasValue)
        {
            query = query.Where(v => v.RegionId == filter.CommuneId);
        }

        if (filter.VillageTypeId.HasValue)
        {
            query = query.Where(v => v.VillageTypeId == filter.VillageTypeId);
        }

        if (filter.IndustryGroupId.HasValue)
        {
            query = query.Where(v => v.IndustryGroupId == filter.IndustryGroupId);
        }

        if (filter.ClassificationId.HasValue)
        {
            query = query.Where(v => v.ClassificationId == filter.ClassificationId);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(v => v.Status == filter.Status);
        }

        if (filter.IsPublished.HasValue)
        {
            query = query.Where(v => v.IsPublished == filter.IsPublished);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.UpdatedAt.HasValue ? v.UpdatedAt : v.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(v => new VillageListDto(
                v.Id,
                v.VillageCode,
                v.Name,
                v.ImageUrl,
                v.Latitude,
                v.Longitude,
                v.Region != null && v.Region.Parent != null ? v.Region.Parent.Name : null,
                v.Region != null ? v.Region.Name : null,
                v.VillageType != null ? v.VillageType.Name : null,
                v.Classification != null ? v.Classification.Name : null,
                v.Status,
                v.IsPublished,
                v.RecognitionDate,
                v.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<VillageListDto>(items, filter.Page, filter.PageSize, totalItems);
    }
}
