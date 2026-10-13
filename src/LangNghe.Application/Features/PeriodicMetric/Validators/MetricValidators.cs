using FluentValidation;
using LangNghe.Application.Features.PeriodicMetric.Dtos;

namespace LangNghe.Application.Features.PeriodicMetric.Validators;

public class CreateMetricCommandValidator : AbstractValidator<CreateMetricDto>
{
    public CreateMetricCommandValidator()
    {
        RuleFor(x => x.CraftVillageId)
            .GreaterThan(0).WithMessage("CraftVillageId phải lớn hơn 0.");

        RuleFor(x => x.ReportPeriod)
            .NotEmpty().WithMessage("Kỳ báo cáo không được để trống.")
            .MaximumLength(20).WithMessage("Kỳ báo cáo không được vượt quá 20 ký tự.");

        RuleFor(x => x.PeriodType)
            .InclusiveBetween((byte)1, (byte)4).WithMessage("Loại kỳ báo cáo không hợp lệ (1: Quý, 2: 6 tháng, 3: Năm).");

        RuleFor(x => x.ProductionProcess)
            .MaximumLength(50).WithMessage("Quy trình sản xuất không được vượt quá 50 ký tự.");

        RuleFor(x => x.ExportRate)
            .InclusiveBetween(0, 100).WithMessage("Tỷ lệ xuất khẩu phải nằm trong khoảng 0 đến 100.");

        RuleFor(x => x.ImportMaterialRate)
            .InclusiveBetween(0, 100).WithMessage("Tỷ lệ nhập khẩu phải nằm trong khoảng 0 đến 100.");

        RuleFor(x => x)
            .Must(x => x.ExportRate + x.ImportMaterialRate <= 100)
            .WithMessage("Tổng tỷ lệ xuất khẩu và tỷ lệ nhập khẩu nguyên liệu không được vượt quá 100%.");

        RuleFor(x => x.HouseholdCount)
            .GreaterThanOrEqualTo(0).WithMessage("Số hộ sản xuất không được âm.");

        RuleFor(x => x.EnterpriseCount)
            .GreaterThanOrEqualTo(0).WithMessage("Số doanh nghiệp không được âm.");

        RuleFor(x => x.ProductionArea)
            .GreaterThanOrEqualTo(0).WithMessage("Diện tích sản xuất không được âm.");

        RuleFor(x => x.ActiveMonths)
            .InclusiveBetween((byte)0, (byte)12).WithMessage("Số tháng hoạt động trong năm phải nằm trong khoảng 0 đến 12.");

        RuleFor(x => x.Output)
            .GreaterThanOrEqualTo(0).WithMessage("Sản lượng không được âm.");

        RuleFor(x => x.Revenue)
            .GreaterThanOrEqualTo(0).WithMessage("Doanh thu không được âm.");

        RuleFor(x => x.TotalLabor)
            .GreaterThanOrEqualTo(0).WithMessage("Tổng số lao động không được âm.");

        RuleFor(x => x.TrainedLaborRate)
            .InclusiveBetween(0, 100).WithMessage("Tỷ lệ lao động qua đào tạo phải nằm trong khoảng 0 đến 100.");

        RuleFor(x => x.ArtisanCount)
            .GreaterThanOrEqualTo(0).WithMessage("Số nghệ nhân không được âm.");

        RuleFor(x => x.AvgIncome)
            .GreaterThanOrEqualTo(0).WithMessage("Thu nhập bình quân không được âm.");

        RuleFor(x => x.DataSource)
            .MaximumLength(50).WithMessage("Nguồn dữ liệu không được vượt quá 50 ký tự.");
    }
}

public class UpdateMetricCommandValidator : AbstractValidator<UpdateMetricDto>
{
    public UpdateMetricCommandValidator()
    {
        RuleFor(x => x.ReportPeriod)
            .NotEmpty().WithMessage("Kỳ báo cáo không được để trống.")
            .MaximumLength(20).WithMessage("Kỳ báo cáo không được vượt quá 20 ký tự.");

        RuleFor(x => x.PeriodType)
            .InclusiveBetween((byte)1, (byte)4).WithMessage("Loại kỳ báo cáo không hợp lệ (1: Quý, 2: 6 tháng, 3: Năm).");

        RuleFor(x => x.ProductionProcess)
            .MaximumLength(50).WithMessage("Quy trình sản xuất không được vượt quá 50 ký tự.");

        RuleFor(x => x.ExportRate)
            .InclusiveBetween(0, 100).WithMessage("Tỷ lệ xuất khẩu phải nằm trong khoảng 0 đến 100.");

        RuleFor(x => x.ImportMaterialRate)
            .InclusiveBetween(0, 100).WithMessage("Tỷ lệ nhập khẩu phải nằm trong khoảng 0 đến 100.");

        RuleFor(x => x)
            .Must(x => x.ExportRate + x.ImportMaterialRate <= 100)
            .WithMessage("Tổng tỷ lệ xuất khẩu và tỷ lệ nhập khẩu nguyên liệu không được vượt quá 100%.");

        RuleFor(x => x.HouseholdCount)
            .GreaterThanOrEqualTo(0).WithMessage("Số hộ sản xuất không được âm.");

        RuleFor(x => x.EnterpriseCount)
            .GreaterThanOrEqualTo(0).WithMessage("Số doanh nghiệp không được âm.");

        RuleFor(x => x.ProductionArea)
            .GreaterThanOrEqualTo(0).WithMessage("Diện tích sản xuất không được âm.");

        RuleFor(x => x.ActiveMonths)
            .InclusiveBetween((byte)0, (byte)12).WithMessage("Số tháng hoạt động trong năm phải nằm trong khoảng 0 đến 12.");

        RuleFor(x => x.Output)
            .GreaterThanOrEqualTo(0).WithMessage("Sản lượng không được âm.");

        RuleFor(x => x.Revenue)
            .GreaterThanOrEqualTo(0).WithMessage("Doanh thu không được âm.");

        RuleFor(x => x.TotalLabor)
            .GreaterThanOrEqualTo(0).WithMessage("Tổng số lao động không được âm.");

        RuleFor(x => x.TrainedLaborRate)
            .InclusiveBetween(0, 100).WithMessage("Tỷ lệ lao động qua đào tạo phải nằm trong khoảng 0 đến 100.");

        RuleFor(x => x.ArtisanCount)
            .GreaterThanOrEqualTo(0).WithMessage("Số nghệ nhân không được âm.");

        RuleFor(x => x.AvgIncome)
            .GreaterThanOrEqualTo(0).WithMessage("Thu nhập bình quân không được âm.");

        RuleFor(x => x.DataSource)
            .MaximumLength(50).WithMessage("Nguồn dữ liệu không được vượt quá 50 ký tự.");
    }
}

