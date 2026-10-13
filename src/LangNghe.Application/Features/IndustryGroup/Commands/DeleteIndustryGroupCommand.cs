using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.IndustryGroup.Commands;

public record DeleteIndustryGroupCommand(int Id) : IRequest<bool>;

public class DeleteIndustryGroupCommandHandler : IRequestHandler<DeleteIndustryGroupCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteIndustryGroupCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteIndustryGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.IndustryGroups
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.IndustryGroup), request.Id);

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
