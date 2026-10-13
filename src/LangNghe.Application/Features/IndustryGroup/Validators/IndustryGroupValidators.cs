using FluentValidation;
using LangNghe.Application.Features.IndustryGroup.Dtos;

namespace LangNghe.Application.Features.IndustryGroup.Validators;

public class CreateIndustryGroupValidator : AbstractValidator<CreateIndustryGroupDto>
{
    public CreateIndustryGroupValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã không được để trống.")
            .MaximumLength(20).WithMessage("Mã không được vượt quá 20 ký tự.")
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Mã chỉ được chứa chữ cái, số và dấu gạch dưới.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(255).WithMessage("Tên không được vượt quá 255 ký tự.");
    }
}

public class UpdateIndustryGroupValidator : AbstractValidator<UpdateIndustryGroupDto>
{
    public UpdateIndustryGroupValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã không được để trống.")
            .MaximumLength(20).WithMessage("Mã không được vượt quá 20 ký tự.")
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Mã chỉ được chứa chữ cái, số và dấu gạch dưới.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(255).WithMessage("Tên không được vượt quá 255 ký tự.");
    }
}
