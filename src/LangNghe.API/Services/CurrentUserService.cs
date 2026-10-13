using System.Security.Claims;
using LangNghe.Domain.Interfaces;

namespace LangNghe.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            return null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public int? DataScope
    {
        get
        {
            var scopeStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("dataScope");
            if (int.TryParse(scopeStr, out int scope))
            {
                return scope;
            }
            return null;
        }
    }

    public int? RegionId
    {
        get
        {
            var regionStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("regionId");
            if (int.TryParse(regionStr, out int region))
            {
                return region;
            }
            return null;
        }
    }

    public int? LevelType
    {
        get
        {
            var levelStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("levelType");
            if (int.TryParse(levelStr, out int level))
            {
                return level;
            }
            return null;
        }
    }

    public IEnumerable<string> Permissions
    {
        get
        {
            var permissions = _httpContextAccessor.HttpContext?.User?.FindAll("permission")
                .Select(c => c.Value)
                .ToArray();

            return permissions ?? Array.Empty<string>();
        }
    }
}
