namespace LangNghe.Infrastructure.Services;

using LangNghe.Application.Interfaces;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Lấy thông tin HttpContext (IP, UserAgent) thông qua IHttpContextAccessor.
/// </summary>
public class HttpContextInfo : IHttpContextInfo
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextInfo(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <inheritdoc />
    public string? IpAddress => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    /// <inheritdoc />
    public string? UserAgent => _accessor.HttpContext?.Request.Headers["User-Agent"].ToString();
}
