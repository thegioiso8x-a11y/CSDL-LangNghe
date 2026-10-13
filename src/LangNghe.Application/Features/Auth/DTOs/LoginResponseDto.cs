namespace LangNghe.Application.Features.Auth.DTOs;

public record LoginResponseDto
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public DateTime AccessExpiresAt { get; init; }
    public UserInfoDto User { get; init; } = null!;
}
