using DomainUser = LangNghe.Domain.Entities.User;

namespace LangNghe.Application.Features.Auth.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(DomainUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    int? ValidateAccessToken(string token);
    (string accessToken, string refreshToken, DateTime accessExpiresAt) IssueTokenPair(DomainUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
}
