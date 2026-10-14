using LangNghe.Application.Features.User.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Queries;

/// <summary>Lấy danh sách vai trò.</summary>
public record GetRolesQuery : IRequest<List<RoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly ILangNgheDbContext _context;
    public GetRolesQueryHandler(ILangNgheDbContext context) => _context = context;
    public Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken) =>
        _context.Roles.AsNoTracking().OrderBy(x => x.Level).ThenBy(x => x.Name)
            .Select(x => new RoleDto(x.Id, x.Code, x.Name, x.Description)).ToListAsync(cancellationToken);
}
