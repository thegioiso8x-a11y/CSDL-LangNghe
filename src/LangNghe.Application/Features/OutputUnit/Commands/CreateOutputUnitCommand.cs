using AutoMapper;
using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.OutputUnit.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.OutputUnit.Commands;

/// <summary>
/// Lệnh tạo mới Đơn vị tính sản lượng.
/// </summary>
public record CreateOutputUnitCommand(CreateOutputUnitDto Dto) : IRequest<int>;

public class CreateOutputUnitCommandHandler : IRequestHandler<CreateOutputUnitCommand, int>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public CreateOutputUnitCommandHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateOutputUnitCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.OutputUnits.AnyAsync(x => x.Code == request.Dto.Code, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại.");

        var entity = _mapper.Map<Domain.Entities.OutputUnit>(request.Dto);

        _context.OutputUnits.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
