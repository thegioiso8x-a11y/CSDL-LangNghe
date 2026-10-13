using AutoMapper;
using LangNghe.Application.Features.VillageType.Dtos;

namespace LangNghe.Application.Features.VillageType.Mappings;

public class VillageTypeProfile : Profile
{
    public VillageTypeProfile()
    {
        CreateMap<CreateVillageTypeDto, Domain.Entities.VillageType>()
            .ForMember(d => d.Id, o => o.Ignore());
            // .ForMember(d => d.CreatedAt, o => o.Ignore()); // Entity doesn't have CreatedAt

        CreateMap<UpdateVillageTypeDto, Domain.Entities.VillageType>()
            .ForMember(d => d.Id, o => o.Ignore());
            // .ForMember(d => d.CreatedAt, o => o.Ignore()); // Entity doesn't have CreatedAt

        CreateMap<Domain.Entities.VillageType, VillageTypeDto>();
    }
}
