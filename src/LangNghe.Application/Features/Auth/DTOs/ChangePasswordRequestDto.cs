namespace LangNghe.Application.Features.Auth.DTOs;

public record ChangePasswordRequestDto
{
    public string OldPassword { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
    public string ConfirmPassword { get; init; } = null!;
}
