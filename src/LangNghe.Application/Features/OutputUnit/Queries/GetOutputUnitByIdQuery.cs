using LangNghe.Application.Features.OutputUnit.Dtos;
using LangNghe.Domain.Interfaces;
using LangNghe.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.OutputUnit.Queries;

/// <summary>
/// Truy vấn Đơn vị tính sản lượng theo ID.
/// </summary>
public record GetOutputUnitByIdQuery(int Id) : IRequest<OutputUnitDto>;

public class GetOutputUnitByIdQueryHandler : IRequestHandler<GetOutputUnitByIdQuery, OutputUnitDto>
{
    private readonly ILangNgheDbContext _context;

    public GetOutputUnitByIdQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<OutputUnitDto> Handle(GetOutputUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.OutputUnits
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new OutputUnitDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.OutputUnit), request.Id);

        return entity;
    }
}
