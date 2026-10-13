using FluentValidation;
using LangNghe.Application.Features.Village.Dtos;

namespace LangNghe.Application.Features.Village.Validators;

public class CreateVillageValidator : AbstractValidator<CreateVillageDto>
{
    public CreateVillageValidator()
    {
        RuleFor(x => x.VillageCode)
            .NotEmpty()
            .Matches(@"^\d{5}$").WithMessage("Mã làng nghề phải có đúng 5 chữ số");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.RegionId)
            .GreaterThan(0);

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.RecognitionDecision)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.RecognitionDecision));

        RuleFor(x => x.ClassificationId)
            .GreaterThan(0)
            .When(x => x.ClassificationId.HasValue);

        RuleFor(x => x.VillageTypeId)
            .GreaterThan(0)
            .When(x => x.VillageTypeId.HasValue);

        RuleFor(x => x.IndustryGroupId)
            .GreaterThan(0)
            .When(x => x.IndustryGroupId.HasValue);
    }
}

public class UpdateVillageValidator : AbstractValidator<UpdateVillageDto>
{
    public UpdateVillageValidator()
    {
        RuleFor(x => x.VillageCode)
            .NotEmpty()
            .Matches(@"^\d{5}$").WithMessage("Mã làng nghề phải có đúng 5 chữ số");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.RegionId)
            .GreaterThan(0);

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.RecognitionDecision)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.RecognitionDecision));

        RuleFor(x => x.ClassificationId)
            .GreaterThan(0)
            .When(x => x.ClassificationId.HasValue);

        RuleFor(x => x.VillageTypeId)
            .GreaterThan(0)
            .When(x => x.VillageTypeId.HasValue);

        RuleFor(x => x.IndustryGroupId)
            .GreaterThan(0)
            .When(x => x.IndustryGroupId.HasValue);

        RuleFor(x => x.Status)
            .Must(x => x == 0 || x == 1 || x == 2)
            .WithMessage("Status không hợp lệ");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty();
    }
}
