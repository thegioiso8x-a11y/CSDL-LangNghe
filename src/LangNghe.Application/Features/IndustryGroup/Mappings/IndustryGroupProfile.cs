using AutoMapper;
using LangNghe.Application.Features.IndustryGroup.Dtos;

namespace LangNghe.Application.Features.IndustryGroup.Mappings;

public class IndustryGroupProfile : Profile
{
    public IndustryGroupProfile()
    {
        CreateMap<CreateIndustryGroupDto, Domain.Entities.IndustryGroup>()
            .ForMember(d => d.Id, o => o.Ignore());
            // .ForMember(d => d.CreatedAt, o => o.Ignore()); // Entity doesn't have CreatedAt

        CreateMap<UpdateIndustryGroupDto, Domain.Entities.IndustryGroup>()
            .ForMember(d => d.Id, o => o.Ignore());
            // .ForMember(d => d.CreatedAt, o => o.Ignore()); // Entity doesn't have CreatedAt

        CreateMap<Domain.Entities.IndustryGroup, IndustryGroupDto>();
    }
}
