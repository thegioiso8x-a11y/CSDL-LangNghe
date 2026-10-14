using LangNghe.Application.Features.User.Dtos;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Commands;

/// <summary>Cập nhật thông tin và vai trò người dùng.</summary>
public record UpdateUserCommand(int Id, UpdateUserDto Dto) : IRequest<bool>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public UpdateUserCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser) => (_context, _currentUser) = (context, currentUser);

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var user = await _context.Users.IgnoreQueryFilters().AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user == null) throw new NotFoundException("User", request.Id);
        if (dto.DataScope is 2 or 3)
        {
            if (!dto.RegionId.HasValue) throw new BusinessException("RegionId là bắt buộc với DataScope 2 hoặc 3");
            if (!await _context.Regions.AnyAsync(x => x.Id == dto.RegionId.Value, cancellationToken)) throw new NotFoundException("Region", dto.RegionId.Value);
        }
        var roleIds = dto.RoleIds.Distinct().ToList();
        if (roleIds.Count != 0 && await _context.Roles.CountAsync(x => roleIds.Contains(x.Id), cancellationToken) != roleIds.Count) throw new NotFoundException("Role", "danh sách được chọn");
        user.FullName = dto.FullName; user.Email = dto.Email; user.Phone = dto.Phone; user.RegionId = dto.RegionId; user.DataScope = dto.DataScope; user.IsActive = dto.IsActive; user.UpdatedAt = DateTime.UtcNow; user.UpdatedBy = _currentUser.UserId;
        var oldRoles = await _context.UserRoles.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(oldRoles);
        foreach (var roleId in roleIds) _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, AssignedAt = DateTime.UtcNow, AssignedBy = _currentUser.UserId });
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
