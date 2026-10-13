using LangNghe.Application.Common;
using FluentValidation;
using LangNghe.Application.Features.Draft.Dtos;
using LangNghe.Application.Features.Draft.Commands;

namespace LangNghe.Application.Features.Draft.Validators;

public class SaveDraftCommandValidator : AbstractValidator<SaveDraftCommand>
{
    public SaveDraftCommandValidator()
    {
        RuleFor(x => x.Dto.DraftData).NotEmpty().WithMessage("DraftData không được để trống.");
    }
}
