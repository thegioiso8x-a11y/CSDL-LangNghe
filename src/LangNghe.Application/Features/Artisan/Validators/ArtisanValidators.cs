using System;
using FluentValidation;
using LangNghe.Application.Features.Artisan.Commands;

namespace LangNghe.Application.Features.Artisan.Validators;

public class CreateArtisanCommandValidator : AbstractValidator<CreateArtisanCommand>
{
    public CreateArtisanCommandValidator()
    {
        var currentYear = DateTime.UtcNow.Year;

        RuleFor(x => x.Dto.FullName)
            .NotEmpty().WithMessage("Tên nghệ nhân không được để trống")
            .MaximumLength(255).WithMessage("Tên nghệ nhân không được vượt quá 255 ký tự");

        RuleFor(x => x.Dto.CraftVillageId)
            .GreaterThan(0).WithMessage("ID làng nghề không hợp lệ");

        When(x => x.Dto.Phone != null, () =>
        {
            RuleFor(x => x.Dto.Phone)
                .Matches(@"^[0-9+\-\s]{6,20}$").WithMessage("Số điện thoại không hợp lệ");
        });

        When(x => x.Dto.BirthYear.HasValue, () =>
        {
            RuleFor(x => x.Dto.BirthYear)
                .InclusiveBetween(1900, currentYear).WithMessage($"Năm sinh phải từ 1900 đến {currentYear}");
        });

        When(x => x.Dto.RecognitionYear.HasValue, () =>
        {
            RuleFor(x => x.Dto.RecognitionYear)
                .InclusiveBetween(1900, currentYear + 1).WithMessage($"Năm phong tặng phải từ 1900 đến {currentYear + 1}");
        });

        When(x => x.Dto.Gender != null, () =>
        {
            RuleFor(x => x.Dto.Gender)
                .MaximumLength(10).WithMessage("Giới tính không được vượt quá 10 ký tự");
        });
    }
}

public class UpdateArtisanCommandValidator : AbstractValidator<UpdateArtisanCommand>
{
    public UpdateArtisanCommandValidator()
    {
        var currentYear = DateTime.UtcNow.Year;

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID không hợp lệ");

        RuleFor(x => x.Dto.FullName)
            .NotEmpty().WithMessage("Tên nghệ nhân không được để trống")
            .MaximumLength(255).WithMessage("Tên nghệ nhân không được vượt quá 255 ký tự");

        RuleFor(x => x.Dto.CraftVillageId)
            .GreaterThan(0).WithMessage("ID làng nghề không hợp lệ");

        When(x => x.Dto.Phone != null, () =>
        {
            RuleFor(x => x.Dto.Phone)
                .Matches(@"^[0-9+\-\s]{6,20}$").WithMessage("Số điện thoại không hợp lệ");
        });

        When(x => x.Dto.BirthYear.HasValue, () =>
        {
            RuleFor(x => x.Dto.BirthYear)
                .InclusiveBetween(1900, currentYear).WithMessage($"Năm sinh phải từ 1900 đến {currentYear}");
        });

        When(x => x.Dto.RecognitionYear.HasValue, () =>
        {
            RuleFor(x => x.Dto.RecognitionYear)
                .InclusiveBetween(1900, currentYear + 1).WithMessage($"Năm phong tặng phải từ 1900 đến {currentYear + 1}");
        });

        When(x => x.Dto.Gender != null, () =>
        {
            RuleFor(x => x.Dto.Gender)
                .MaximumLength(10).WithMessage("Giới tính không được vượt quá 10 ký tự");
        });
    }
}
