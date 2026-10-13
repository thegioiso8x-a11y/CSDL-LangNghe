using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LangNghe.Application.Features.Region.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Region.Commands;

public record UpdateRegionCommand(int Id, UpdateRegionDto Dto) : IRequest<bool>;

public class UpdateRegionCommandHandler : IRequestHandler<UpdateRegionCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public UpdateRegionCommandHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdateRegionCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var entity = await _context.Regions
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Region), request.Id);

        if (entity.RegionCode != dto.RegionCode && await _context.Regions.AnyAsync(x => x.RegionCode == dto.RegionCode && x.Id != request.Id, cancellationToken))
            throw new BusinessException($"Mã địa phương '{dto.RegionCode}' đã tồn tại.");

        if (entity.LevelType != dto.LevelType)
            throw new BusinessException("Không được đổi cấp hành chính.");

        if (entity.Children.Count > 0 && entity.ParentId != dto.ParentId)
            throw new BusinessException("Không thể đổi cấp trên khi địa phương này đang có cấp dưới trực thuộc.");

        if (dto.LevelType == 2)
        {
            if (!dto.ParentId.HasValue)
                throw new BusinessException("Xã/Phường phải thuộc một Tỉnh/Thành phố (ParentId không được trống).");

            var parent = await _context.Regions.FirstOrDefaultAsync(x => x.Id == dto.ParentId.Value, cancellationToken);
            if (parent == null)
                throw new BusinessException("Tỉnh/Thành phố cấp trên không tồn tại.");

            if (parent.LevelType != 1)
                throw new BusinessException("Tỉnh/Thành phố cấp trên không hợp lệ (LevelType phải là 1).");
        }
        else if (dto.LevelType == 1)
        {
            if (dto.ParentId.HasValue)
                throw new BusinessException("Tỉnh/Thành phố không được có cấp trên (ParentId phải null).");
        }

        _mapper.Map(dto, entity);
        
        _context.Regions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
