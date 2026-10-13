using AutoMapper;
using LangNghe.Application.Features.Region.Dtos;

namespace LangNghe.Application.Features.Region.Mappings;

public class RegionProfile : Profile
{
    public RegionProfile()
    {
        CreateMap<CreateRegionDto, Domain.Entities.Region>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateRegionDto, Domain.Entities.Region>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
