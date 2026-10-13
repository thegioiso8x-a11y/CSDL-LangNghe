using FluentValidation;
using LangNghe.Application.Features.Auth.DTOs;
using LangNghe.Application.Features.Auth.Interfaces;
using MediatR;

namespace LangNghe.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(int UserId, ChangePasswordRequestDto Dto) : IRequest<bool>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        await _authService.ChangePasswordAsync(request.UserId, request.Dto, cancellationToken);
        return true;
    }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Dto.OldPassword).NotEmpty().WithMessage("Mật khẩu cũ không được để trống");
        RuleFor(x => x.Dto.NewPassword).NotEmpty().WithMessage("Mật khẩu mới không được để trống")
                                       .MinimumLength(8).WithMessage("Mật khẩu mới phải có ít nhất 8 ký tự")
                                       .NotEqual(x => x.Dto.OldPassword).WithMessage("Mật khẩu mới phải khác mật khẩu cũ");
        RuleFor(x => x.Dto.ConfirmPassword).Equal(x => x.Dto.NewPassword).WithMessage("Xác nhận mật khẩu không khớp");
    }
}
