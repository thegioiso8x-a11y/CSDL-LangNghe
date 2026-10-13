using AutoMapper;
using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.ExportCategory.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.ExportCategory.Commands;

/// <summary>
/// Lệnh tạo mới Nhóm mặt hàng xuất khẩu.
/// </summary>
public record CreateExportCategoryCommand(CreateExportCategoryDto Dto) : IRequest<int>;

public class CreateExportCategoryCommandHandler : IRequestHandler<CreateExportCategoryCommand, int>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public CreateExportCategoryCommandHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateExportCategoryCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.ExportCategories.AnyAsync(x => x.Code == request.Dto.Code, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại.");

        var entity = _mapper.Map<Domain.Entities.ExportCategory>(request.Dto);

        _context.ExportCategories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
