using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Workflow.Commands;

public record PublishWorkflowCommand(long Id) : IRequest<ApiResponse<bool>>;

public class PublishWorkflowCommandHandler : IRequestHandler<PublishWorkflowCommand, ApiResponse<bool>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PublishWorkflowCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(PublishWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.DataScope != 1) // Chỉ TW
            throw new UnauthorizedAccessException("Chỉ cấp TW mới có quyền công bố.");

        var workflow = await _context.SubmissionWorkflows
            .AsTracking()
            .Include(x => x.CraftVillage)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (workflow == null)
            throw new NotFoundException("SubmissionWorkflow không tồn tại", request.Id);

        if (workflow.CurrentStatus != 2)
            throw new BusinessException("Chỉ có thể công bố khi trạng thái là ApprovedProvince (2).");

        workflow.CurrentStatus = 3; // Published
        workflow.PublishedByTW = _currentUser.UserId;
        workflow.PublishedAt = System.DateTime.UtcNow;

        if (workflow.CraftVillage != null)
        {
            workflow.CraftVillage.IsPublished = true;
            workflow.CraftVillage.PublishedAt = System.DateTime.UtcNow;
            workflow.CraftVillage.Status = 1;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }
}
