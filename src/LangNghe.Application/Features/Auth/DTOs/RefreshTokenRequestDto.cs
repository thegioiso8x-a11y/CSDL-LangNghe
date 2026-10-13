namespace LangNghe.Application.Features.Auth.DTOs;

public record RefreshTokenRequestDto
{
    public string RefreshToken { get; init; } = null!;
}
