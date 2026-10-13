namespace LangNghe.Application.Features.Auth.DTOs;

public record LoginRequestDto
{
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string CaptchaId { get; init; } = null!;
    public string CaptchaAnswer { get; init; } = null!;
}
