using FluentValidation;
using LangNghe.Application.Features.Auth.DTOs;
using LangNghe.Application.Features.Auth.Interfaces;
using MediatR;

namespace LangNghe.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequestDto Dto, string IpAddress) : IRequest<LoginResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Dto, request.IpAddress, cancellationToken);
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Dto.Username).NotEmpty().WithMessage("Tên đăng nhập không được để trống");
        RuleFor(x => x.Dto.Password).NotEmpty().WithMessage("Mật khẩu không được để trống")
                                    .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự");
        RuleFor(x => x.Dto.CaptchaId).NotEmpty().WithMessage("CaptchaId không được để trống");
        RuleFor(x => x.Dto.CaptchaAnswer).NotEmpty().WithMessage("Câu trả lời Captcha không được để trống");
    }
}
