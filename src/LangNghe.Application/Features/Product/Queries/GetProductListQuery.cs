using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Product.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Product.Queries;

public record GetProductListQuery(ProductFilterDto Dto) : IRequest<PagedResult<ProductDto>>;

public class GetProductListQueryHandler : IRequestHandler<GetProductListQuery, PagedResult<ProductDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetProductListQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
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
            query = query.Where(x => x.Name.Contains(keyword) || (x.ProductCode != null && x.ProductCode.Contains(keyword)));
        }

        if (request.Dto.HasOCOP.HasValue)
        {
            query = query.Where(x => x.HasOCOP == request.Dto.HasOCOP.Value);
        }

        if (request.Dto.OcopStar.HasValue)
        {
            query = query.Where(x => x.OcopStar == request.Dto.OcopStar.Value);
        }

        if (request.Dto.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.Dto.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((request.Dto.Page - 1) * request.Dto.PageSize)
            .Take(request.Dto.PageSize)
            .Select(x => new ProductDto(
                x.Id,
                x.CraftVillageId,
                x.CraftVillage != null ? x.CraftVillage.Name : null,
                x.ProductCode,
                x.Name,
                x.Description,
                x.Materials,
                x.ImageUrl,
                x.HasOCOP,
                x.OcopStar,
                x.OcopEvaluatedYear,
                x.MainMarket,
                x.SortOrder,
                x.IsActive,
                x.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>(items, request.Dto.Page, request.Dto.PageSize, totalCount);
    }
}
