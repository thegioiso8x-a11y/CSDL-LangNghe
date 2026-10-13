using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Artisan.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Artisan.Commands;

public record CreateArtisanCommand(CreateArtisanDto Dto) : IRequest<int>;

public class CreateArtisanCommandHandler : IRequestHandler<CreateArtisanCommand, int>
{
    private readonly ILangNgheDbContext _context;

    public CreateArtisanCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateArtisanCommand request, CancellationToken cancellationToken)
    {
        var villageExists = await _context.CraftVillages.AnyAsync(x => x.Id == request.Dto.CraftVillageId, cancellationToken);
        if (!villageExists)
            throw new NotFoundException("Không tìm thấy làng nghề", request.Dto.CraftVillageId);

        var entity = new Domain.Entities.Artisan
        {
            CraftVillageId = request.Dto.CraftVillageId,
            FullName = request.Dto.FullName,
            BirthYear = request.Dto.BirthYear,
            Gender = request.Dto.Gender,
            ArtisanLevel = request.Dto.ArtisanLevel,
            RecognitionYear = request.Dto.RecognitionYear,
            Specialty = request.Dto.Specialty,
            Phone = request.Dto.Phone,
            IsActive = true
        };

        _context.Artisans.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
