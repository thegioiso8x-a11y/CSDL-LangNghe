using AutoMapper;
using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.Classification.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Classification.Commands;

public record CreateClassificationCommand(CreateClassificationDto Dto) : IRequest<int>;

public class CreateClassificationCommandHandler : IRequestHandler<CreateClassificationCommand, int>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public CreateClassificationCommandHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateClassificationCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Classifications.AnyAsync(x => x.Code == request.Dto.Code, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại.");

        var entity = _mapper.Map<Domain.Entities.Classification>(request.Dto);

        _context.Classifications.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
