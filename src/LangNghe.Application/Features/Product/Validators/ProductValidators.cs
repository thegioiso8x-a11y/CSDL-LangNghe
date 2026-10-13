using FluentValidation;
using LangNghe.Application.Features.Product.Commands;

namespace LangNghe.Application.Features.Product.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
            .MaximumLength(500).WithMessage("Tên sản phẩm không được vượt quá 500 ký tự");

        RuleFor(x => x.Dto.ProductCode)
            .MaximumLength(50).WithMessage("Mã sản phẩm không được vượt quá 50 ký tự");

        RuleFor(x => x.Dto.CraftVillageId)
            .GreaterThan(0).WithMessage("ID làng nghề không hợp lệ");

        When(x => x.Dto.HasOCOP, () =>
        {
            RuleFor(x => x.Dto.OcopStar)
                .NotNull().WithMessage("Sản phẩm OCOP phải có số sao")
                .Must(x => x == 3 || x == 4 || x == 5).WithMessage("Số sao OCOP phải là 3, 4 hoặc 5");
        });

        When(x => !x.Dto.HasOCOP, () =>
        {
            RuleFor(x => x.Dto.OcopStar)
                .Null().WithMessage("Sản phẩm không phải OCOP thì không được có số sao");
        });
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID không hợp lệ");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
            .MaximumLength(500).WithMessage("Tên sản phẩm không được vượt quá 500 ký tự");

        RuleFor(x => x.Dto.ProductCode)
            .MaximumLength(50).WithMessage("Mã sản phẩm không được vượt quá 50 ký tự");

        RuleFor(x => x.Dto.CraftVillageId)
            .GreaterThan(0).WithMessage("ID làng nghề không hợp lệ");

        When(x => x.Dto.HasOCOP, () =>
        {
            RuleFor(x => x.Dto.OcopStar)
                .NotNull().WithMessage("Sản phẩm OCOP phải có số sao")
                .Must(x => x == 3 || x == 4 || x == 5).WithMessage("Số sao OCOP phải là 3, 4 hoặc 5");
        });

        When(x => !x.Dto.HasOCOP, () =>
        {
            RuleFor(x => x.Dto.OcopStar)
                .Null().WithMessage("Sản phẩm không phải OCOP thì không được có số sao");
        });
    }
}
