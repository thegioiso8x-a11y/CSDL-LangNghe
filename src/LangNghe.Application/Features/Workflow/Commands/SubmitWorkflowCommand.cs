using LangNghe.Application.Common;
using System.Threading;
using System.Threading.Tasks;
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

    public SubmitWorkflowCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUser, IVillageOwnershipService ownershipService)
    {
        _context = context;
        _currentUser = currentUser;
        _ownershipService = ownershipService;
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

        return ApiResponse<long>.Ok(newWorkflow.Id);
    }
}
