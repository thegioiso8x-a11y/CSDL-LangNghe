using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Constants;
using LangNghe.Application.Features.Notifications.Interfaces;
using LangNghe.Application.Features.Workflow.Dtos;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Workflow.Commands;

public record SubmitWorkflowCommand(SubmitWorkflowDto Dto) : IRequest<ApiResponse<long>>;

public class SubmitWorkflowCommandHandler : IRequestHandler<SubmitWorkflowCommand, ApiResponse<long>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IVillageOwnershipService _ownershipService;
    private readonly INotificationService _notificationService;

    public SubmitWorkflowCommandHandler(
        ILangNgheDbContext context,
        ICurrentUserService currentUser,
        IVillageOwnershipService ownershipService,
        INotificationService notificationService)
    {
        _context = context;
        _currentUser = currentUser;
        _ownershipService = ownershipService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<long>> Handle(SubmitWorkflowCommand request, CancellationToken cancellationToken)
    {
        await _ownershipService.ValidateVillageAccessAsync(request.Dto.CraftVillageId, cancellationToken);
        
        // Phải có Metric kỳ đó
        var hasMetric = await _context.VillagePeriodicMetrics
            .AnyAsync(m => m.CraftVillageId == request.Dto.CraftVillageId && m.ReportPeriod == request.Dto.ReportPeriod, cancellationToken);
            
        if (!hasMetric)
            throw new BusinessException($"Làng nghề chưa có dữ liệu báo cáo định kỳ cho kỳ {request.Dto.ReportPeriod}");

        var existingWorkflow = await _context.SubmissionWorkflows
            .AsTracking()
            .Include(x => x.CraftVillage!)
            .ThenInclude(x => x.Region!)
            .FirstOrDefaultAsync(x => x.CraftVillageId == request.Dto.CraftVillageId && x.ReportPeriod == request.Dto.ReportPeriod, cancellationToken);

        if (existingWorkflow != null)
            {
            // Chỉ submit khi status 0/9
            if (existingWorkflow.CurrentStatus != 0 && existingWorkflow.CurrentStatus != 9)
                throw new BusinessException("Trạng thái hiện tại không cho phép trình duyệt.");
                
            existingWorkflow.CurrentStatus = 1;
            existingWorkflow.SentToProvinceAt = System.DateTime.UtcNow;
            existingWorkflow.Notes = request.Dto.Notes;
            existingWorkflow.Version += 1;
            
            await _context.SaveChangesAsync(cancellationToken);
            await NotifyProvinceAsync(existingWorkflow, cancellationToken);
            return ApiResponse<long>.Ok(existingWorkflow.Id);
        }

        var newWorkflow = new SubmissionWorkflow
        {
            CraftVillageId = request.Dto.CraftVillageId,
            ReportPeriod = request.Dto.ReportPeriod,
            CurrentStatus = 1,
            CreatedByXa = _currentUser.UserId ?? throw new UnauthorizedAccessException(),
            CreatedAt = System.DateTime.UtcNow,
            SentToProvinceAt = System.DateTime.UtcNow,
            Notes = request.Dto.Notes,
            Version = 1
        };

        _context.SubmissionWorkflows.Add(newWorkflow);
        await _context.SaveChangesAsync(cancellationToken);
        await NotifyProvinceAsync(newWorkflow, cancellationToken);

        return ApiResponse<long>.Ok(newWorkflow.Id);
    }

    private async Task NotifyProvinceAsync(SubmissionWorkflow workflow, CancellationToken cancellationToken)
    {
        var village = workflow.CraftVillage;
        if (village?.Region is null)
        {
            village = await _context.CraftVillages
                .Include(x => x.Region)
                .FirstOrDefaultAsync(x => x.Id == workflow.CraftVillageId, cancellationToken);
        }

        if (village?.Region is null)
            return;

        var provinceId = village.Region.ParentId ?? village.RegionId;
        var userIds = await _context.Users
            .Where(u => u.DataScope == 2 && u.IsActive && u.RegionId == provinceId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        await _notificationService.CreateBulkAsync(
            userIds,
            "Có báo cáo mới chờ duyệt",
            $"Làng nghề {village.Name} đã gửi báo cáo kỳ {workflow.ReportPeriod}",
            NotificationTypes.WorkflowSubmit,
            "SubmissionWorkflow",
            workflow.Id.ToString(),
            $"/submissions/{workflow.Id}",
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
