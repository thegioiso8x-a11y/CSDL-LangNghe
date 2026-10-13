using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LangNghe.Application.Features.Auth.Interfaces;
using LangNghe.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LangNghe.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var accessTokenMinutesStr = _configuration["Jwt:AccessTokenMinutes"];
        var accessTokenMinutes = int.TryParse(accessTokenMinutesStr, out int min) ? min : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("dataScope", user.DataScope.ToString())
        };

        if (user.RegionId.HasValue)
        {
            claims.Add(new Claim("regionId", user.RegionId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessTokenMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public (string accessToken, string refreshToken, DateTime accessExpiresAt) IssueTokenPair(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var accessToken = GenerateAccessToken(user, roles, permissions);
        var refreshToken = GenerateRefreshToken();
        
        var accessTokenMinutesStr = _configuration["Jwt:AccessTokenMinutes"];
        var accessTokenMinutes = int.TryParse(accessTokenMinutesStr, out int min) ? min : 60;
        var accessExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenMinutes);

        return (accessToken, refreshToken, accessExpiresAt);
    }

    public int? ValidateAccessToken(string token)
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey!);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdStr = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
