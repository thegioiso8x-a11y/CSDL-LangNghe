using AutoMapper;
using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.IndustryGroup.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.IndustryGroup.Commands;

public record CreateIndustryGroupCommand(CreateIndustryGroupDto Dto) : IRequest<int>;

public class CreateIndustryGroupCommandHandler : IRequestHandler<CreateIndustryGroupCommand, int>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public CreateIndustryGroupCommandHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateIndustryGroupCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.IndustryGroups.AnyAsync(x => x.Code == request.Dto.Code, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại.");

        var entity = _mapper.Map<Domain.Entities.IndustryGroup>(request.Dto);

        _context.IndustryGroups.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
