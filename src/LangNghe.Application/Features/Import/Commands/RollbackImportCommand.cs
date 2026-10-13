namespace LangNghe.Application.Features.Import.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record RollbackImportCommand(Guid BatchId) : IRequest<bool>;

public class RollbackImportCommandHandler : IRequestHandler<RollbackImportCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RollbackImportCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(RollbackImportCommand request, CancellationToken cancellationToken)
    {
        var history = await _context.ImportHistories
            .FirstOrDefaultAsync(x => x.Id == request.BatchId, cancellationToken);

        if (history == null)
            throw new NotFoundException(nameof(history), request.BatchId);

        if (history.Status != 1) // 1 = Completed
            throw new BusinessException("Chỉ rollback batch đã Completed");

        history.Status = 3; // 3 = RolledBack
        history.ErrorSummary = $"Đã rollback bởi user {_currentUserService.UserId} lúc {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
