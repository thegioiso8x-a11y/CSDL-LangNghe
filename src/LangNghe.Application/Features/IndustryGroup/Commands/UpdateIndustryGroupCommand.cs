using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.IndustryGroup.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.IndustryGroup.Commands;

public record UpdateIndustryGroupCommand(int Id, UpdateIndustryGroupDto Dto) : IRequest<bool>;

public class UpdateIndustryGroupCommandHandler : IRequestHandler<UpdateIndustryGroupCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public UpdateIndustryGroupCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateIndustryGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.IndustryGroups
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.IndustryGroup), request.Id);

        var exists = await _context.IndustryGroups.AnyAsync(x => x.Code == request.Dto.Code && x.Id != request.Id, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại ở bản ghi khác.");

        entity.Code = request.Dto.Code;
        entity.Name = request.Dto.Name;
        entity.Description = request.Dto.Description;
        entity.SortOrder = request.Dto.SortOrder;
        entity.IsActive = request.Dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
