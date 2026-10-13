using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Product.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Product.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly ILangNgheDbContext _context;

    public GetProductByIdQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(x => x.CraftVillage)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Không tìm thấy sản phẩm", request.Id);

        return new ProductDto(
            product.Id,
            product.CraftVillageId,
            product.CraftVillage?.Name,
            product.ProductCode,
            product.Name,
            product.Description,
            product.Materials,
            product.ImageUrl,
            product.HasOCOP,
            product.OcopStar,
            product.OcopEvaluatedYear,
            product.MainMarket,
            product.SortOrder,
            product.IsActive,
            product.UpdatedAt
        );
    }
}
