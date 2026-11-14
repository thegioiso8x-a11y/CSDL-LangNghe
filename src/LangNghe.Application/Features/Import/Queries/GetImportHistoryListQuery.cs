namespace LangNghe.Application.Features.Import.Queries;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Domain.Interfaces;
using LangNghe.Application.Features.Import.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetImportHistoryListQuery(ImportFilterDto Filter) : IRequest<PagedResult<ImportResultDto>>;

public class GetImportHistoryListQueryHandler : IRequestHandler<GetImportHistoryListQuery, PagedResult<ImportResultDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetImportHistoryListQueryHandler(ILangNgheDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<ImportResultDto>> Handle(GetImportHistoryListQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.ImportHistories.AsNoTracking();

        // Lọc theo DataScope để tránh rò rỉ dữ liệu giữa các tỉnh/xã.
        var dataScope = _currentUserService.DataScope;
        var userRegionId = _currentUserService.RegionId;

        if (dataScope == 2 && userRegionId != null)
        {
            // Tỉnh: chỉ xem lịch sử do Tỉnh mình upload HOẶC do các Xã thuộc Tỉnh mình upload.
            query = query.Where(x =>
                x.UploadedByRegionId == userRegionId
                || (x.UploadedByRegion != null && x.UploadedByRegion.ParentId == userRegionId));
        }
        else if (dataScope == 3 && userRegionId != null)
        {
            // Xã: chỉ xem lịch sử do chính Xã mình upload.
            query = query.Where(x => x.UploadedByRegionId == userRegionId);
        }
        else if (dataScope != 1)
        {
            // Không xác định được DataScope (null hoặc giá trị lạ) → không trả dữ liệu nào.
            query = query.Where(x => false);
        }

        if (filter.BatchId.HasValue)
        {
            query = query.Where(x => x.Id == filter.BatchId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(x => x.StartedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(x => x.StartedAt <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.StartedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ImportResultDto(
                x.Id,
                x.TotalRows,
                x.SuccessRows,
                x.ErrorRows,
                x.WarningRows,
                x.Status,
                x.ErrorSummary,
                x.FileName,
                x.TemplateCode,
                x.ImportMode,
                x.StartedAt,
                x.CompletedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ImportResultDto>(items, filter.Page, filter.PageSize, totalCount);
    }
}

