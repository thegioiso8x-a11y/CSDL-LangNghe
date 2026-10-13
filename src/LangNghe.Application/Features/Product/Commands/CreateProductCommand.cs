using System;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Product.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Product.Commands;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<int>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly ILangNgheDbContext _context;

    public CreateProductCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var villageExists = await _context.CraftVillages.AnyAsync(x => x.Id == request.Dto.CraftVillageId, cancellationToken);
        if (!villageExists)
            throw new NotFoundException("Không tìm thấy làng nghề", request.Dto.CraftVillageId);

        if (!string.IsNullOrWhiteSpace(request.Dto.ProductCode))
        {
            var exists = await _context.Products.AnyAsync(
                x => x.CraftVillageId == request.Dto.CraftVillageId && x.ProductCode == request.Dto.ProductCode, 
                cancellationToken);
            if (exists)
                throw new BusinessException($"Mã sản phẩm '{request.Dto.ProductCode}' đã tồn tại trong làng nghề này");
        }

        if (request.Dto.HasOCOP)
        {
            if (!request.Dto.OcopStar.HasValue || (request.Dto.OcopStar.Value != 3 && request.Dto.OcopStar.Value != 4 && request.Dto.OcopStar.Value != 5))
                throw new BusinessException("Sản phẩm OCOP phải có số sao 3, 4 hoặc 5");
        }
        else
        {
            if (request.Dto.OcopStar.HasValue)
                throw new BusinessException("Sản phẩm không phải OCOP thì không được có số sao");
        }

        var entity = new Domain.Entities.Product
        {
            CraftVillageId = request.Dto.CraftVillageId,
            ProductCode = request.Dto.ProductCode,
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            Materials = request.Dto.Materials,
            ImageUrl = request.Dto.ImageUrl,
            HasOCOP = request.Dto.HasOCOP,
            OcopStar = request.Dto.OcopStar,
            OcopEvaluatedYear = request.Dto.OcopEvaluatedYear,
            MainMarket = request.Dto.MainMarket,
            SortOrder = request.Dto.SortOrder,
            IsActive = true
        };

        _context.Products.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
