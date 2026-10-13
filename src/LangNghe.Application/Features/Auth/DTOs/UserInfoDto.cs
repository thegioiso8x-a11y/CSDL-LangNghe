namespace LangNghe.Application.Features.Auth.DTOs;

public record UserInfoDto
{
    public int Id { get; init; }
    public string Username { get; init; } = null!;
    public string? FullName { get; init; }
    public byte DataScope { get; init; }
    public int? RegionId { get; init; }
    public string[] Roles { get; init; } = Array.Empty<string>();
    public string[] Permissions { get; init; } = Array.Empty<string>();
}
