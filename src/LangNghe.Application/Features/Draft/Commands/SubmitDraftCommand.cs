using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Draft.Commands;

public record SubmitDraftCommand(long Id) : IRequest<ApiResponse<bool>>;

public class SubmitDraftCommandHandler : IRequestHandler<SubmitDraftCommand, ApiResponse<bool>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SubmitDraftCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(SubmitDraftCommand request, CancellationToken cancellationToken)
    {
        var draft = await _context.VillageDrafts
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CreatedByUserId == _currentUser.UserId, cancellationToken);

        if (draft == null)
            throw new NotFoundException("VillageDraft không tồn tại hoặc không thuộc quyền sở hữu", request.Id);

        draft.DraftStatus = 1; // Submitted status for draft
        draft.SubmittedAt = System.DateTime.UtcNow;
        draft.UpdatedAt = System.DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }
}
