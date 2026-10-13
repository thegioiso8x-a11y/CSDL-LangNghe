using FluentValidation;
using LangNghe.Application.Features.ExportCategory.Dtos;

namespace LangNghe.Application.Features.ExportCategory.Validators;

public class CreateExportCategoryValidator : AbstractValidator<CreateExportCategoryDto>
{
    public CreateExportCategoryValidator()
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

public class UpdateExportCategoryValidator : AbstractValidator<UpdateExportCategoryDto>
{
    public UpdateExportCategoryValidator()
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
