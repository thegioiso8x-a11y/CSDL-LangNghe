using LangNghe.Application.Features.User.Dtos;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Commands;

/// <summary>Thay thế toàn bộ vai trò của người dùng.</summary>
public record AssignRolesCommand(int Id, AssignRolesDto Dto) : IRequest<bool>;

public class AssignRolesCommandHandler : IRequestHandler<AssignRolesCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public AssignRolesCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser) => (_context, _currentUser) = (context, currentUser);
    public async Task<bool> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        if (!await _context.Users.IgnoreQueryFilters().AnyAsync(x => x.Id == request.Id, cancellationToken)) throw new NotFoundException("User", request.Id);
        var roleIds = request.Dto.RoleIds.Distinct().ToList();
        if (roleIds.Count != 0 && await _context.Roles.CountAsync(x => roleIds.Contains(x.Id), cancellationToken) != roleIds.Count) throw new NotFoundException("Role", "danh sách được chọn");
        var oldRoles = await _context.UserRoles.Where(x => x.UserId == request.Id).ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(oldRoles);
        foreach (var roleId in roleIds) _context.UserRoles.Add(new UserRole { UserId = request.Id, RoleId = roleId, AssignedAt = DateTime.UtcNow, AssignedBy = _currentUser.UserId });
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
