using LangNghe.Application.Features.Auth.DTOs;

namespace LangNghe.Application.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, string ip, CancellationToken ct);
    Task<LoginResponseDto> RefreshAsync(string refreshToken, string ip, CancellationToken ct);
    Task RevokeAsync(string refreshToken, string ip, CancellationToken ct);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto, CancellationToken ct);
}
