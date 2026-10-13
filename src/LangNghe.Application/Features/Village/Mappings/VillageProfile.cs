using AutoMapper;
using LangNghe.Application.Features.Village.Dtos;
using LangNghe.Domain.Entities;

namespace LangNghe.Application.Features.Village.Mappings;

public class VillageProfile : Profile
{
    public VillageProfile()
    {
        CreateMap<CraftVillage, VillageListDto>()
            .ForMember(d => d.ProvinceName, opt => opt.MapFrom(s => s.Region != null && s.Region.Parent != null ? s.Region.Parent.Name : null))
            .ForMember(d => d.CommuneName, opt => opt.MapFrom(s => s.Region != null ? s.Region.Name : null))
            .ForMember(d => d.VillageTypeName, opt => opt.MapFrom(s => s.VillageType != null ? s.VillageType.Name : null))
            .ForMember(d => d.ClassificationName, opt => opt.MapFrom(s => s.Classification != null ? s.Classification.Name : null));

        CreateMap<CraftVillage, VillageDetailDto>()
            .ForMember(d => d.ProvinceName, opt => opt.MapFrom(s => s.Region != null && s.Region.Parent != null ? s.Region.Parent.Name : null))
            .ForMember(d => d.CommuneName, opt => opt.MapFrom(s => s.Region != null ? s.Region.Name : null))
            .ForMember(d => d.VillageTypeName, opt => opt.MapFrom(s => s.VillageType != null ? s.VillageType.Name : null))
            .ForMember(d => d.ClassificationName, opt => opt.MapFrom(s => s.Classification != null ? s.Classification.Name : null))
            .ForMember(d => d.IndustryGroupName, opt => opt.MapFrom(s => s.IndustryGroup != null ? s.IndustryGroup.Name : null))
            .ForMember(d => d.RowVersion, opt => opt.MapFrom(s => s.RowVersion != null ? Convert.ToBase64String(s.RowVersion) : string.Empty));
    }
}
