using LangNghe.Application.Common;
using FluentValidation;
using LangNghe.Application.Features.Workflow.Dtos;
using LangNghe.Application.Features.Workflow.Commands;

namespace LangNghe.Application.Features.Workflow.Validators;

public class RejectWorkflowCommandValidator : AbstractValidator<RejectWorkflowCommand>
{
    public RejectWorkflowCommandValidator()
    {
        RuleFor(x => x.Dto.Reason).NotEmpty().WithMessage("Lý do từ chối không được để trống.");
    }
}

public class SubmitWorkflowCommandValidator : AbstractValidator<SubmitWorkflowCommand>
{
    public SubmitWorkflowCommandValidator()
    {
        RuleFor(x => x.Dto.CraftVillageId).GreaterThan(0).WithMessage("CraftVillageId không hợp lệ.");
        RuleFor(x => x.Dto.ReportPeriod).NotEmpty().WithMessage("ReportPeriod không được để trống.");
    }
}
