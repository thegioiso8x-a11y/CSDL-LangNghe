using LangNghe.Application.Features.Auth.DTOs;
using LangNghe.Application.Features.Auth.Interfaces;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using LangNghe.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LangNghe.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly LangNgheDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly ICaptchaService _captchaService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        LangNgheDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtService,
        ICaptchaService captchaService,
        IDateTimeProvider dateTime,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _captchaService = captchaService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, string ip, CancellationToken ct)
    {
        if (!_captchaService.Validate(dto.CaptchaId, dto.CaptchaAnswer))
        {
            throw new BusinessException("Captcha không đúng");
        }

        var user = await _context.Users
            .AsTracking()
            .IgnoreQueryFilters()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == dto.Username.Trim(), ct);

        if (user == null)
        {
            throw new BusinessException("Tên đăng nhập hoặc mật khẩu không đúng");
        }

        if (!user.IsActive)
        {
            throw new BusinessException("Tài khoản đã bị khóa");
        }

        var now = _dateTime.UtcNow;
        if (user.LockedUntil.HasValue && user.LockedUntil.Value > now)
        {
            throw new BusinessException($"Tài khoản bị khóa đến {user.LockedUntil:HH:mm dd/MM/yyyy}");
        }

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                user.LockedUntil = now.AddMinutes(15);
                user.FailedLoginCount = 0;
            }
            await _context.SaveChangesAsync(ct);
            throw new BusinessException("Tên đăng nhập hoặc mật khẩu không đúng");
        }

        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = now;
        user.LastLoginIp = ip;

        var roles = user.UserRoles.Select(ur => ur.Role!.Code).ToList();
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var permissions = await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToListAsync(ct);

        var (access, refresh, expiresAt) = _jwtService.IssueTokenPair(user, roles, permissions);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refresh,
            ExpiresAt = now.AddDays(7),
            CreatedAt = now,
            CreatedByIp = ip
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(ct);

        var userInfoDto = new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            DataScope = user.DataScope,
            RegionId = user.RegionId,
            Roles = roles.ToArray(),
            Permissions = permissions.ToArray()
        };

        return new LoginResponseDto
        {
            AccessToken = access,
            RefreshToken = refresh,
            AccessExpiresAt = expiresAt,
            User = userInfoDto
        };
    }

    public async Task<LoginResponseDto> RefreshAsync(string refreshToken, string ip, CancellationToken ct)
    {
        var now = _dateTime.UtcNow;
        var existing = await _context.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (existing == null || existing.RevokedAt != null || existing.ExpiresAt < now)
        {
            throw new BusinessException("Refresh token không hợp lệ hoặc đã hết hạn");
        }

        var user = await _context.Users
            .AsTracking()
            .IgnoreQueryFilters()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == existing.UserId, ct);

        if (user == null || !user.IsActive)
        {
            throw new BusinessException("Tài khoản đã bị khóa");
        }

        existing.RevokedAt = now;
        existing.RevokedByIp = ip;

        var roles = user.UserRoles.Select(ur => ur.Role!.Code).ToList();
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var permissions = await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToListAsync(ct);

        var (access, refresh, expiresAt) = _jwtService.IssueTokenPair(user, roles, permissions);

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refresh,
            ExpiresAt = now.AddDays(7),
            CreatedAt = now,
            CreatedByIp = ip
        };

        existing.ReplacedBy = refresh;
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(ct);

        var userInfoDto = new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            DataScope = user.DataScope,
            RegionId = user.RegionId,
            Roles = roles.ToArray(),
            Permissions = permissions.ToArray()
        };

        return new LoginResponseDto
        {
            AccessToken = access,
            RefreshToken = refresh,
            AccessExpiresAt = expiresAt,
            User = userInfoDto
        };
    }

    public async Task RevokeAsync(string refreshToken, string ip, CancellationToken ct)
    {
        var existing = await _context.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (existing == null)
        {
            throw new NotFoundException("RefreshToken", refreshToken);
        }

        existing.RevokedAt = _dateTime.UtcNow;
        existing.RevokedByIp = ip;

        await _context.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto, CancellationToken ct)
    {
        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        if (!_passwordHasher.Verify(dto.OldPassword, user.PasswordHash))
        {
            throw new BusinessException("Mật khẩu cũ không đúng");
        }

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.MustChangePassword = false;

        var now = _dateTime.UtcNow;

        await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);

        await _context.SaveChangesAsync(ct);
    }
}
