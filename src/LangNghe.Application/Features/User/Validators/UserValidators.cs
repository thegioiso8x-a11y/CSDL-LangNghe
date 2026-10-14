using FluentValidation;
using LangNghe.Application.Features.User.Commands;

namespace LangNghe.Application.Features.User.Validators;

public static class UserValidationRules
{
    public static IRuleBuilderOptions<T, string> Username<T>(IRuleBuilder<T, string> rule) => rule.NotEmpty().Length(3, 100).Matches("^[a-zA-Z0-9_]+$").WithMessage("Username chỉ gồm chữ, số và dấu gạch dưới");
    public static IRuleBuilderOptions<T, string> Password<T>(IRuleBuilder<T, string> rule) => rule.NotEmpty().MinimumLength(8).Matches("[A-Za-z]").Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ và số");
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        UserValidationRules.Username(RuleFor(x => x.Dto.Username)); UserValidationRules.Password(RuleFor(x => x.Dto.Password));
        RuleFor(x => x.Dto.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Dto.Email)).WithMessage("Email không hợp lệ");
        RuleFor(x => x.Dto.Phone).Matches("^(0|\\+84)(3|5|7|8|9)[0-9]{8}$").When(x => !string.IsNullOrWhiteSpace(x.Dto.Phone)).WithMessage("Số điện thoại không hợp lệ");
        RuleFor(x => x.Dto.DataScope).Must(x => x is 1 or 2 or 3).WithMessage("DataScope chỉ nhận giá trị 1, 2 hoặc 3");
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0); RuleFor(x => x.Dto.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Dto.Email)).WithMessage("Email không hợp lệ");
        RuleFor(x => x.Dto.Phone).Matches("^(0|\\+84)(3|5|7|8|9)[0-9]{8}$").When(x => !string.IsNullOrWhiteSpace(x.Dto.Phone)).WithMessage("Số điện thoại không hợp lệ");
        RuleFor(x => x.Dto.DataScope).Must(x => x is 1 or 2 or 3).WithMessage("DataScope chỉ nhận giá trị 1, 2 hoặc 3");
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator() { RuleFor(x => x.Id).GreaterThan(0); UserValidationRules.Password(RuleFor(x => x.Dto.NewPassword)); }
}

public class AssignRolesCommandValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesCommandValidator() { RuleFor(x => x.Id).GreaterThan(0); RuleFor(x => x.Dto.RoleIds).NotNull(); }
}
