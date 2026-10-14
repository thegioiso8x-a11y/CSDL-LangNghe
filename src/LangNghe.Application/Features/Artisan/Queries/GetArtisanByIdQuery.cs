using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Artisan.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Artisan.Queries;

public record GetArtisanByIdQuery(int Id) : IRequest<ArtisanDto>;

public class GetArtisanByIdQueryHandler : IRequestHandler<GetArtisanByIdQuery, ArtisanDto>
{
    private readonly ILangNgheDbContext _context;

    public GetArtisanByIdQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<ArtisanDto> Handle(GetArtisanByIdQuery request, CancellationToken cancellationToken)
    {
        var artisan = await _context.Artisans
            .AsNoTracking()
            .Include(x => x.CraftVillage)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (artisan == null)
            throw new NotFoundException("Không tìm thấy nghệ nhân", request.Id);

        return new ArtisanDto(
            artisan.Id,
            artisan.CraftVillageId,
            artisan.CraftVillage?.Name,
            artisan.FullName,
            artisan.BirthYear,
            artisan.Gender,
            artisan.ArtisanLevel,
            artisan.RecognitionYear,
            artisan.Specialty,
            artisan.Phone,
            artisan.ImageUrl,
            artisan.IsActive,
            artisan.UpdatedAt
        );
    }
}
