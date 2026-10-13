using FluentValidation;
using LangNghe.Application.Features.Classification.Dtos;

namespace LangNghe.Application.Features.Classification.Validators;

public class CreateClassificationValidator : AbstractValidator<CreateClassificationDto>
{
    public CreateClassificationValidator()
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

public class UpdateClassificationValidator : AbstractValidator<UpdateClassificationDto>
{
    public UpdateClassificationValidator()
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
