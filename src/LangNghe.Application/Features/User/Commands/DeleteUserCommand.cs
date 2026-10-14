using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Commands;

/// <summary>Vô hiệu hóa mềm một người dùng.</summary>
public record DeleteUserCommand(int Id) : IRequest<bool>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public DeleteUserCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser) => (_context, _currentUser) = (context, currentUser);
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == request.Id) throw new BusinessException("Không được tự xóa chính mình");
        var user = await _context.Users.IgnoreQueryFilters().AsTracking().Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user == null) throw new NotFoundException("User", request.Id);
        if (user.UserRoles.Any(x => x.Role?.Code == "ADMIN_TW")) throw new BusinessException("Không được xóa người dùng có role ADMIN_TW");
        user.IsActive = false; user.UpdatedAt = DateTime.UtcNow; user.UpdatedBy = _currentUser.UserId;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
