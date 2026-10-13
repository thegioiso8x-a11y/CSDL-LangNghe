namespace LangNghe.Application.Features.Auth.DTOs;

public record CaptchaResponseDto
{
    public string CaptchaId { get; init; } = null!;
    public string Question { get; init; } = null!;
}
