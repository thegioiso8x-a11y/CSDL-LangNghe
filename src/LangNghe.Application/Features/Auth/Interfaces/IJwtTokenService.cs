using LangNghe.Domain.Entities;

namespace LangNghe.Application.Features.Auth.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    int? ValidateAccessToken(string token);
    (string accessToken, string refreshToken, DateTime accessExpiresAt) IssueTokenPair(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
}
