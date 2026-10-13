using LangNghe.Application.Features.Village.Dtos;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LangNghe.Application.Features.Village.Commands;

public record CreateVillageCommand(CreateVillageDto Dto, int? UserRequesting = null) : IRequest<int>;

public class CreateVillageCommandHandler : IRequestHandler<CreateVillageCommand, int>
{
    private readonly ILangNgheDbContext _context;
    private readonly IVillageOwnershipService _ownershipService;
    private readonly ICurrentUserService _currentUserService;

    public CreateVillageCommandHandler(ILangNgheDbContext context, IVillageOwnershipService ownershipService, ICurrentUserService currentUserService)
    {
        _context = context;
        _ownershipService = ownershipService;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(CreateVillageCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Check VillageCode unique
        var exists = await _context.CraftVillages.AnyAsync(x => x.VillageCode == dto.VillageCode, cancellationToken);
        if (exists)
        {
            throw new BusinessException("Mã làng nghề đã tồn tại");
        }

        // 2. Validate regex ^\d{5}$
        if (!Regex.IsMatch(dto.VillageCode, @"^\d{5}$"))
        {
            throw new BusinessException("Mã làng nghề phải có đúng 5 chữ số");
        }

        // 3. Ownership check
        await _ownershipService.ValidateCreateAsync(dto.RegionId, dto.VillageCode, cancellationToken);

        // 4. Set mặc định + Map
        var entity = new CraftVillage
        {
            VillageCode = dto.VillageCode,
            Name = dto.Name,
            RegionId = dto.RegionId,
            Address = dto.Address,
            RecognitionDecision = dto.RecognitionDecision,
            RecognitionDate = dto.RecognitionDate,
            ClassificationId = dto.ClassificationId,
            VillageTypeId = dto.VillageTypeId,
            IndustryGroupId = dto.IndustryGroupId,
            NeedRestoration = dto.NeedRestoration,
            Status = 0,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId
        };

        _context.CraftVillages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
