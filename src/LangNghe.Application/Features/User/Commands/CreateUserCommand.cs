using LangNghe.Application.Features.Auth.Interfaces;
using LangNghe.Application.Features.User.Dtos;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Commands;

/// <summary>Tạo người dùng mới.</summary>
public record CreateUserCommand(CreateUserDto Dto) : IRequest<int>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly ILangNgheDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    public CreateUserCommandHandler(ILangNgheDbContext context, IPasswordHasher passwordHasher, ICurrentUserService currentUser)
        => (_context, _passwordHasher, _currentUser) = (context, passwordHasher, currentUser);

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var username = dto.Username.Trim();
        if (await _context.Users.IgnoreQueryFilters().AnyAsync(x => x.Username.ToLower() == username.ToLower(), cancellationToken))
            throw new BusinessException("Tên đăng nhập đã tồn tại");
        if (dto.DataScope is 2 or 3)
        {
            if (!dto.RegionId.HasValue) throw new BusinessException("RegionId là bắt buộc với DataScope 2 hoặc 3");
            if (!await _context.Regions.AnyAsync(x => x.Id == dto.RegionId.Value, cancellationToken))
                throw new NotFoundException("Region", dto.RegionId.Value);
        }
        var roleIds = dto.RoleIds.Distinct().ToList();
        if (roleIds.Count != 0 && await _context.Roles.CountAsync(x => roleIds.Contains(x.Id), cancellationToken) != roleIds.Count)
            throw new NotFoundException("Role", "danh sách được chọn");

        var user = new Domain.Entities.User
        {
            Username = username, PasswordHash = _passwordHasher.Hash(dto.Password), FullName = dto.FullName,
            Email = dto.Email, Phone = dto.Phone, RegionId = dto.RegionId, DataScope = dto.DataScope,
            MustChangePassword = dto.MustChangePassword, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = _currentUser.UserId
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        foreach (var roleId in roleIds) _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, AssignedAt = DateTime.UtcNow, AssignedBy = _currentUser.UserId });
        await _context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
