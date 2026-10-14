using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Constants;
using LangNghe.Application.Features.Notifications.Interfaces;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Workflow.Commands;

public record ApproveWorkflowCommand(long Id) : IRequest<ApiResponse<bool>>;

public class ApproveWorkflowCommandHandler : IRequestHandler<ApproveWorkflowCommand, ApiResponse<bool>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public ApproveWorkflowCommandHandler(
        ILangNgheDbContext context,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<bool>> Handle(ApproveWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.DataScope != 2) // Chỉ Tỉnh
            throw new ForbiddenException("Chỉ cấp Tỉnh mới có quyền phê duyệt.");

        var workflow = await _context.SubmissionWorkflows
            .AsTracking()
            .Include(x => x.CraftVillage!)
            .ThenInclude(x => x.Region!)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (workflow == null)
            throw new NotFoundException("SubmissionWorkflow không tồn tại", request.Id);

        if (workflow.CurrentStatus != 1)
            throw new BusinessException("Chỉ có thể phê duyệt khi trạng thái là PendingProvince (1).");

        if (workflow.CraftVillage?.Region?.ParentId != _currentUser.RegionId)
            throw new ForbiddenException("Làng nghề không thuộc quản lý của Tỉnh này.");

        workflow.CurrentStatus = 2; // ApprovedProvince
        workflow.ApprovedByTinh = _currentUser.UserId;
        workflow.ApprovedByTinhAt = System.DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var villageName = workflow.CraftVillage?.Name ?? "làng nghề";
        await _notificationService.CreateAsync(
            workflow.CreatedByXa,
            "Báo cáo đã được Tỉnh phê duyệt",
            $"Báo cáo làng nghề {villageName} kỳ {workflow.ReportPeriod} đã được Sở phê duyệt, chờ TW công bố",
            NotificationTypes.WorkflowApprove,
            "SubmissionWorkflow",
            workflow.Id.ToString(),
            $"/submissions/{workflow.Id}",
            cancellationToken);

        var twUserIds = await _context.Users
            .Where(u => u.DataScope == 1 && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        await _notificationService.CreateBulkAsync(
            twUserIds,
            "Có báo cáo chờ công bố",
            null,
            NotificationTypes.WorkflowApprove,
            "SubmissionWorkflow",
            workflow.Id.ToString(),
            $"/submissions/{workflow.Id}",
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }
}
