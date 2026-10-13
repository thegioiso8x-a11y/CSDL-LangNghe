using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Draft.Commands;

public record DeleteDraftCommand(long Id) : IRequest<ApiResponse<bool>>;

public class DeleteDraftCommandHandler : IRequestHandler<DeleteDraftCommand, ApiResponse<bool>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteDraftCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteDraftCommand request, CancellationToken cancellationToken)
    {
        var draft = await _context.VillageDrafts
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CreatedByUserId == _currentUser.UserId, cancellationToken);

        if (draft == null)
            throw new NotFoundException("VillageDraft không tồn tại hoặc không thuộc quyền sở hữu", request.Id);

        _context.VillageDrafts.Remove(draft);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }
}
