using FluentValidation;
using LangNghe.Application.Features.OutputUnit.Dtos;

namespace LangNghe.Application.Features.OutputUnit.Validators;

public class CreateOutputUnitValidator : AbstractValidator<CreateOutputUnitDto>
{
    public CreateOutputUnitValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã không được để trống.")
            .MaximumLength(20).WithMessage("Mã không được vượt quá 20 ký tự.")
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Mã chỉ được chứa chữ cái, số và dấu gạch dưới.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự.");
    }
}

public class UpdateOutputUnitValidator : AbstractValidator<UpdateOutputUnitDto>
{
    public UpdateOutputUnitValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã không được để trống.")
            .MaximumLength(20).WithMessage("Mã không được vượt quá 20 ký tự.")
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Mã chỉ được chứa chữ cái, số và dấu gạch dưới.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự.");
    }
}
