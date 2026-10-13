using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Artisan.Commands;

public record DeleteArtisanCommand(int Id) : IRequest<bool>;

public class DeleteArtisanCommandHandler : IRequestHandler<DeleteArtisanCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteArtisanCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteArtisanCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Artisans
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("Không tìm thấy nghệ nhân", request.Id);

        _context.Artisans.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
