using FluentValidation;
using LangNghe.Application.Features.Auth.Interfaces;
using MediatR;

namespace LangNghe.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string RefreshToken, string IpAddress) : IRequest<bool>;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
{
    private readonly IAuthService _authService;

    public RevokeTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        await _authService.RevokeAsync(request.RefreshToken, request.IpAddress, cancellationToken);
        return true;
    }
}

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh Token không được để trống");
    }
}
