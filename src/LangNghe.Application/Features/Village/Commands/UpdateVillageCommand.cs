using LangNghe.Application.Features.Village.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LangNghe.Application.Features.Village.Commands;

public record UpdateVillageCommand(int Id, UpdateVillageDto Dto) : IRequest<bool>;

public class UpdateVillageCommandHandler : IRequestHandler<UpdateVillageCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateVillageCommandHandler(ILangNgheDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateVillageCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Find AsTracking
        var entity = await _context.CraftVillages.AsTracking()
            .Include(v => v.Region)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("CraftVillage", request.Id);
        }

        // 2. Optimistic lock check
        var currentRowVersion = entity.RowVersion != null ? Convert.ToBase64String(entity.RowVersion) : string.Empty;
        if (dto.RowVersionBase64 != currentRowVersion)
        {
            throw new BusinessException("Dữ liệu đã bị thay đổi bởi người khác, vui lòng tải lại trang");
        }

        // 3. Check VillageCode unique trừ chính nó
        if (entity.VillageCode != dto.VillageCode)
        {
            var exists = await _context.CraftVillages.AnyAsync(x => x.VillageCode == dto.VillageCode && x.Id != request.Id, cancellationToken);
            if (exists)
            {
                throw new BusinessException("Mã làng nghề đã tồn tại");
            }

            if (!Regex.IsMatch(dto.VillageCode, @"^\d{5}$"))
            {
                throw new BusinessException("Mã làng nghề phải có đúng 5 chữ số");
            }
        }

        // 4. Update các field
        entity.VillageCode = dto.VillageCode;
        entity.Name = dto.Name;
        entity.RegionId = dto.RegionId;
        entity.Address = dto.Address;
        entity.ImageUrl = dto.ImageUrl;
        entity.RecognitionDecision = dto.RecognitionDecision;
        entity.RecognitionDate = dto.RecognitionDate;
        entity.ClassificationId = dto.ClassificationId;
        entity.VillageTypeId = dto.VillageTypeId;
        entity.IndustryGroupId = dto.IndustryGroupId;
        entity.NeedRestoration = dto.NeedRestoration;
        entity.Status = dto.Status;
        
        // 5. UpdatedAt = UtcNow
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _currentUserService.UserId;

        // 6. Save
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
