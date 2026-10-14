using LangNghe.Application.Common;
using LangNghe.Application.Features.User.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Queries;

/// <summary>Lấy danh sách người dùng theo bộ lọc và phân trang.</summary>
public record GetUserListQuery(UserFilterDto Dto) : IRequest<PagedResult<UserDto>>;

public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, PagedResult<UserDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetUserListQueryHandler(ILangNgheDbContext context) => _context = context;

    public async Task<PagedResult<UserDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.IgnoreQueryFilters().AsNoTracking().AsQueryable();
        var dto = request.Dto;

        if (!string.IsNullOrWhiteSpace(dto.Keyword))
        {
            var keyword = dto.Keyword.Trim();
            query = query.Where(x => x.Username.Contains(keyword) ||
                (x.FullName != null && x.FullName.Contains(keyword)) ||
                (x.Email != null && x.Email.Contains(keyword)));
        }
        if (dto.RegionId.HasValue) query = query.Where(x => x.RegionId == dto.RegionId.Value);
        if (dto.DataScope.HasValue) query = query.Where(x => x.DataScope == dto.DataScope.Value);
        if (dto.IsActive.HasValue) query = query.Where(x => x.IsActive == dto.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(dto.RoleCode))
        {
            var roleCode = dto.RoleCode.Trim();
            query = query.Where(x => x.UserRoles.Any(ur => ur.Role != null && ur.Role.Code == roleCode));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((dto.Page - 1) * dto.PageSize).Take(dto.PageSize)
            .Select(x => new UserDto(x.Id, x.Username, x.FullName, x.Email, x.Phone,
                x.RegionId, x.Region != null ? x.Region.Name : null, x.DataScope,
                x.DataScope == 1 ? "Toàn quốc" : x.DataScope == 2 ? "Tỉnh" : "Xã",
                x.IsActive, x.LastLoginAt,
                x.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Code).ToList(), x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>(items, dto.Page, dto.PageSize, totalCount);
    }
}
