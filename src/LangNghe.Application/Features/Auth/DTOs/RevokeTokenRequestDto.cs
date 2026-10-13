namespace LangNghe.Application.Features.Auth.DTOs;

public record RevokeTokenRequestDto
{
    public string RefreshToken { get; init; } = null!;
}
