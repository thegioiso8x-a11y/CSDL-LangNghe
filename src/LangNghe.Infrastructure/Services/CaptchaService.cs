using LangNghe.Application.Features.Auth.DTOs;
using LangNghe.Application.Features.Auth.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace LangNghe.Infrastructure.Services;

public class CaptchaService : ICaptchaService
{
    private readonly IMemoryCache _cache;
    private readonly Random _random = new();

    public CaptchaService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public CaptchaResponseDto Generate()
    {
        var a = _random.Next(1, 21);
        var b = _random.Next(1, 21);
        var isAddition = _random.Next(0, 2) == 0;
        
        var answer = isAddition ? a + b : a * b;
        var operation = isAddition ? "+" : "×";
        var captchaId = Guid.NewGuid().ToString("N");

        _cache.Set($"captcha:{captchaId}", answer, TimeSpan.FromMinutes(5));

        return new CaptchaResponseDto
        {
            CaptchaId = captchaId,
            Question = $"{a} {operation} {b} = ?"
        };
    }

    public bool Validate(string captchaId, string answer)
    {
        if (_cache.TryGetValue($"captcha:{captchaId}", out int cachedAnswer))
        {
            _cache.Remove($"captcha:{captchaId}");
            if (int.TryParse(answer, out int parsedAnswer) && parsedAnswer == cachedAnswer)
            {
                return true;
            }
        }
        return false;
    }
}
