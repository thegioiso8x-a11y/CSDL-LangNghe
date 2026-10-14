using System;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Artisan.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Artisan.Commands;

public record UpdateArtisanCommand(int Id, UpdateArtisanDto Dto) : IRequest<bool>;

public class UpdateArtisanCommandHandler : IRequestHandler<UpdateArtisanCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public UpdateArtisanCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateArtisanCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Artisans
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("Không tìm thấy nghệ nhân", request.Id);

        var villageExists = await _context.CraftVillages.AnyAsync(x => x.Id == request.Dto.CraftVillageId, cancellationToken);
        if (!villageExists)
            throw new NotFoundException("Không tìm thấy làng nghề", request.Dto.CraftVillageId);

        entity.CraftVillageId = request.Dto.CraftVillageId;
        entity.FullName = request.Dto.FullName;
        entity.BirthYear = request.Dto.BirthYear;
        entity.Gender = request.Dto.Gender;
        entity.ArtisanLevel = request.Dto.ArtisanLevel;
        entity.RecognitionYear = request.Dto.RecognitionYear;
        entity.Specialty = request.Dto.Specialty;
        entity.Phone = request.Dto.Phone;
        entity.ImageUrl = request.Dto.ImageUrl;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
