using System;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Product.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Product.Commands;

public record UpdateProductCommand(int Id, UpdateProductDto Dto) : IRequest<bool>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public UpdateProductCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Products
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("Không tìm thấy sản phẩm", request.Id);

        var villageExists = await _context.CraftVillages.AnyAsync(x => x.Id == request.Dto.CraftVillageId, cancellationToken);
        if (!villageExists)
            throw new NotFoundException("Không tìm thấy làng nghề", request.Dto.CraftVillageId);

        if (!string.IsNullOrWhiteSpace(request.Dto.ProductCode))
        {
            var exists = await _context.Products.AnyAsync(
                x => x.CraftVillageId == request.Dto.CraftVillageId && 
                     x.ProductCode == request.Dto.ProductCode && 
                     x.Id != request.Id, 
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

        entity.CraftVillageId = request.Dto.CraftVillageId;
        entity.ProductCode = request.Dto.ProductCode;
        entity.Name = request.Dto.Name;
        entity.Description = request.Dto.Description;
        entity.Materials = request.Dto.Materials;
        entity.ImageUrl = request.Dto.ImageUrl;
        entity.HasOCOP = request.Dto.HasOCOP;
        entity.OcopStar = request.Dto.OcopStar;
        entity.OcopEvaluatedYear = request.Dto.OcopEvaluatedYear;
        entity.MainMarket = request.Dto.MainMarket;
        entity.SortOrder = request.Dto.SortOrder;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
