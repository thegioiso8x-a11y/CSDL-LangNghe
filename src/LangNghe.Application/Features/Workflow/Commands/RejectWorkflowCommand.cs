using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Workflow.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Workflow.Commands;

public record RejectWorkflowCommand(long Id, RejectWorkflowDto Dto) : IRequest<ApiResponse<bool>>;

public class RejectWorkflowCommandHandler : IRequestHandler<RejectWorkflowCommand, ApiResponse<bool>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RejectWorkflowCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(RejectWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.DataScope != 2) // Chỉ Tỉnh
            throw new ForbiddenException("Chỉ cấp Tỉnh mới có quyền từ chối.");

        var workflow = await _context.SubmissionWorkflows
            .AsTracking()
            .Include(x => x.CraftVillage!)
            .ThenInclude(x => x.Region!)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (workflow == null)
            throw new NotFoundException("SubmissionWorkflow không tồn tại", request.Id);

        if (workflow.CurrentStatus != 1)
            throw new BusinessException("Chỉ có thể từ chối khi trạng thái là PendingProvince (1).");

        if (workflow.CraftVillage?.Region?.ParentId != _currentUser.RegionId)
            throw new ForbiddenException("Làng nghề không thuộc quản lý của Tỉnh này.");

        if (string.IsNullOrWhiteSpace(request.Dto.Reason))
            throw new BusinessException("Phải nhập lý do từ chối.");

        workflow.CurrentStatus = 9; // Rejected
        workflow.RejectedBy = _currentUser.UserId;
        workflow.RejectedAt = System.DateTime.UtcNow;
        workflow.RejectionReason = request.Dto.Reason;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }
}
