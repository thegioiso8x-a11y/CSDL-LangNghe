using LangNghe.Application.Features.Auth.Interfaces;
using LangNghe.Application.Features.User.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Commands;

/// <summary>Đặt lại mật khẩu cho người dùng.</summary>
public record ResetPasswordCommand(int Id, ResetPasswordDto Dto) : IRequest<bool>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    public ResetPasswordCommandHandler(ILangNgheDbContext context, IPasswordHasher passwordHasher) => (_context, _passwordHasher) = (context, passwordHasher);
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.IgnoreQueryFilters().AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user == null) throw new NotFoundException("User", request.Id);
        user.PasswordHash = _passwordHasher.Hash(request.Dto.NewPassword); user.MustChangePassword = true;
        var now = DateTime.UtcNow;
        await _context.RefreshTokens.Where(x => x.UserId == request.Id && x.RevokedAt == null).ExecuteUpdateAsync(x => x.SetProperty(t => t.RevokedAt, now), cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
