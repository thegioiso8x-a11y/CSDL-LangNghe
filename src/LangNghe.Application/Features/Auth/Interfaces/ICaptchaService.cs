using LangNghe.Application.Features.Auth.DTOs;

namespace LangNghe.Application.Features.Auth.Interfaces;

public interface ICaptchaService
{
    CaptchaResponseDto Generate();
    bool Validate(string captchaId, string answer);
}
