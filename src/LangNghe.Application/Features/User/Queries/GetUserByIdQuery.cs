using LangNghe.Application.Features.User.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Queries;

/// <summary>Lấy chi tiết một người dùng.</summary>
public record GetUserByIdQuery(int Id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly ILangNgheDbContext _context;
    public GetUserByIdQueryHandler(ILangNgheDbContext context) => _context = context;

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.IgnoreQueryFilters().AsNoTracking()
            .Include(x => x.Region).Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user == null) throw new NotFoundException("User", request.Id);
        return new UserDto(user.Id, user.Username, user.FullName, user.Email, user.Phone,
            user.RegionId, user.Region?.Name, user.DataScope,
            user.DataScope == 1 ? "Toàn quốc" : user.DataScope == 2 ? "Tỉnh" : "Xã",
            user.IsActive, user.LastLoginAt, user.UserRoles.Where(x => x.Role != null).Select(x => x.Role!.Code).ToList(), user.CreatedAt);
    }
}
