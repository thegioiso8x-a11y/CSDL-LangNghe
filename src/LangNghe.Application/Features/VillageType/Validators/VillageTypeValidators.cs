using FluentValidation;
using LangNghe.Application.Features.VillageType.Dtos;

namespace LangNghe.Application.Features.VillageType.Validators;

public class CreateVillageTypeValidator : AbstractValidator<CreateVillageTypeDto>
{
    public CreateVillageTypeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã không được để trống.")
            .MaximumLength(20).WithMessage("Mã không được vượt quá 20 ký tự.")
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Mã chỉ được chứa chữ cái, số và dấu gạch dưới.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(500).WithMessage("Tên không được vượt quá 500 ký tự.");
    }
}

public class UpdateVillageTypeValidator : AbstractValidator<UpdateVillageTypeDto>
{
    public UpdateVillageTypeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã không được để trống.")
            .MaximumLength(20).WithMessage("Mã không được vượt quá 20 ký tự.")
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Mã chỉ được chứa chữ cái, số và dấu gạch dưới.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(500).WithMessage("Tên không được vượt quá 500 ký tự.");
    }
}
