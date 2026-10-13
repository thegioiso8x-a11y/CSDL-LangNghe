using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LangNghe.Application.Features.Region.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Region.Commands;

public record CreateRegionCommand(CreateRegionDto Dto) : IRequest<int>;

public class CreateRegionCommandHandler : IRequestHandler<CreateRegionCommand, int>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public CreateRegionCommandHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateRegionCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (await _context.Regions.AnyAsync(x => x.RegionCode == dto.RegionCode, cancellationToken))
            throw new BusinessException($"Mã địa phương '{dto.RegionCode}' đã tồn tại.");

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

        var entity = _mapper.Map<Domain.Entities.Region>(dto);
        entity.IsActive = true;

        _context.Regions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
