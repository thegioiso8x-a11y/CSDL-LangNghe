using FluentValidation;
using LangNghe.Application.Features.Region.Commands;
using LangNghe.Application.Features.Region.Dtos;

namespace LangNghe.Application.Features.Region.Validators;

public class CreateRegionValidator : AbstractValidator<CreateRegionCommand>
{
    public CreateRegionValidator()
    {
        RuleFor(v => v.Dto.RegionCode)
            .NotEmpty().WithMessage("Mã địa phương không được để trống.")
            .MaximumLength(20).WithMessage("Mã địa phương không được vượt quá 20 ký tự.")
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Mã địa phương chỉ chứa chữ cái, số và dấu gạch dưới.");

        RuleFor(v => v.Dto.Name)
            .NotEmpty().WithMessage("Tên địa phương không được để trống.")
            .MaximumLength(255).WithMessage("Tên địa phương không được vượt quá 255 ký tự.");

        RuleFor(v => v.Dto.ShortName)
            .MaximumLength(100).WithMessage("Tên ngắn không được vượt quá 100 ký tự.")
            .When(v => !string.IsNullOrEmpty(v.Dto.ShortName));

        RuleFor(v => v.Dto.LevelType)
            .Must(x => x == 1 || x == 2).WithMessage("LevelType chỉ nhận 1 hoặc 2.");
    }
}

public class UpdateRegionValidator : AbstractValidator<UpdateRegionCommand>
{
    public UpdateRegionValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id không hợp lệ.");

        RuleFor(v => v.Dto.RegionCode)
            .NotEmpty().WithMessage("Mã địa phương không được để trống.")
            .MaximumLength(20).WithMessage("Mã địa phương không được vượt quá 20 ký tự.")
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Mã địa phương chỉ chứa chữ cái, số và dấu gạch dưới.");

        RuleFor(v => v.Dto.Name)
            .NotEmpty().WithMessage("Tên địa phương không được để trống.")
            .MaximumLength(255).WithMessage("Tên địa phương không được vượt quá 255 ký tự.");

        RuleFor(v => v.Dto.ShortName)
            .MaximumLength(100).WithMessage("Tên ngắn không được vượt quá 100 ký tự.")
            .When(v => !string.IsNullOrEmpty(v.Dto.ShortName));

        RuleFor(v => v.Dto.LevelType)
            .Must(x => x == 1 || x == 2).WithMessage("LevelType chỉ nhận 1 hoặc 2.");
    }
}
