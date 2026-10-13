using AutoMapper;
using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.VillageType.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.VillageType.Commands;

public record CreateVillageTypeCommand(CreateVillageTypeDto Dto) : IRequest<int>;

public class CreateVillageTypeCommandHandler : IRequestHandler<CreateVillageTypeCommand, int>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public CreateVillageTypeCommandHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateVillageTypeCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.VillageTypes.AnyAsync(x => x.Code == request.Dto.Code, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại.");

        var entity = _mapper.Map<Domain.Entities.VillageType>(request.Dto);

        _context.VillageTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
