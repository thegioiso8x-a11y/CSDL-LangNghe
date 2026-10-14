using LangNghe.Application.Features.User.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Queries;

/// <summary>Lấy danh sách quyền, sắp xếp và nhóm theo module.</summary>
public record GetPermissionsQuery : IRequest<List<PermissionDto>>;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
{
    private readonly ILangNgheDbContext _context;
    public GetPermissionsQueryHandler(ILangNgheDbContext context) => _context = context;
    public Task<List<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken) =>
        _context.Permissions.AsNoTracking().OrderBy(x => x.Module).ThenBy(x => x.Action)
            .Select(x => new PermissionDto(x.Id, x.Code, x.Module, x.Action, x.Description)).ToListAsync(cancellationToken);
}
