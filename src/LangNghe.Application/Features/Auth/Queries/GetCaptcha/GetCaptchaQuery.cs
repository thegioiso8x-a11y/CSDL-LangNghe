using LangNghe.Application.Features.Auth.DTOs;
using LangNghe.Application.Features.Auth.Interfaces;
using MediatR;

namespace LangNghe.Application.Features.Auth.Queries.GetCaptcha;

public record GetCaptchaQuery : IRequest<CaptchaResponseDto>;

public class GetCaptchaQueryHandler : IRequestHandler<GetCaptchaQuery, CaptchaResponseDto>
{
    private readonly ICaptchaService _captchaService;

    public GetCaptchaQueryHandler(ICaptchaService captchaService)
    {
        _captchaService = captchaService;
    }

    public Task<CaptchaResponseDto> Handle(GetCaptchaQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_captchaService.Generate());
    }
}
