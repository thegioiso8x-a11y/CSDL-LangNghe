using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Village.Commands;

public record PublishVillageCommand(int Id, bool Publish) : IRequest<bool>;

public class PublishVillageCommandHandler : IRequestHandler<PublishVillageCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public PublishVillageCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(PublishVillageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CraftVillages.AsTracking()
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("CraftVillage", request.Id);
        }

        if (request.Publish)
        {
            entity.IsPublished = true;
            entity.PublishedAt = DateTime.UtcNow;
            entity.Status = 1;
        }
        else
        {
            entity.IsPublished = false;
            entity.PublishedAt = null;
            entity.Status = 0;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
