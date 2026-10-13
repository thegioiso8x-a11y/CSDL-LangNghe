using FluentValidation;
using LangNghe.Application.Features.Auth.DTOs;
using LangNghe.Application.Features.Auth.Interfaces;
using MediatR;

namespace LangNghe.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string IpAddress) : IRequest<LoginResponseDto>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RefreshAsync(request.RefreshToken, request.IpAddress, cancellationToken);
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh Token không được để trống");
    }
}
