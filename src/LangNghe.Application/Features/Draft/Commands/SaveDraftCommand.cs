using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Draft.Dtos;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Interfaces;
using LangNghe.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Draft.Commands;

public record SaveDraftCommand(SaveDraftDto Dto) : IRequest<ApiResponse<long>>;

public class SaveDraftCommandHandler : IRequestHandler<SaveDraftCommand, ApiResponse<long>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SaveDraftCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<long>> Handle(SaveDraftCommand request, CancellationToken cancellationToken)
    {
        // Upsert theo user + village
        var existingDraft = await _context.VillageDrafts
            .AsTracking()
            .FirstOrDefaultAsync(x => x.CreatedByUserId == _currentUser.UserId && x.CraftVillageId == request.Dto.CraftVillageId, cancellationToken);

        if (existingDraft != null)
        {
            existingDraft.DraftData = request.Dto.DraftData;
            existingDraft.DraftVillageCode = request.Dto.DraftVillageCode;
            existingDraft.UpdatedAt = System.DateTime.UtcNow;
            existingDraft.AutoSavedAt = System.DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<long>.Ok(existingDraft.Id);
        }

        var newDraft = new VillageDraft
        {
            CraftVillageId = request.Dto.CraftVillageId,
            DraftVillageCode = request.Dto.DraftVillageCode,
            DraftData = request.Dto.DraftData,
            CreatedByUserId = _currentUser.UserId ?? throw new UnauthorizedException("Không xác định được người dùng hiện tại"),
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow,
            AutoSavedAt = System.DateTime.UtcNow,
            DraftStatus = 0 // Nháp
        };

        _context.VillageDrafts.Add(newDraft);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<long>.Ok(newDraft.Id);
    }
}
