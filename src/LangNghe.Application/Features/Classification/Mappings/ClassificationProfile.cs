using AutoMapper;
using LangNghe.Application.Features.Classification.Dtos;

namespace LangNghe.Application.Features.Classification.Mappings;

public class ClassificationProfile : Profile
{
    public ClassificationProfile()
    {
        CreateMap<CreateClassificationDto, Domain.Entities.Classification>()
            .ForMember(d => d.Id, o => o.Ignore());
            // .ForMember(d => d.CreatedAt, o => o.Ignore()); // Entity doesn't have CreatedAt

        CreateMap<UpdateClassificationDto, Domain.Entities.Classification>()
            .ForMember(d => d.Id, o => o.Ignore());
            // .ForMember(d => d.CreatedAt, o => o.Ignore()); // Entity doesn't have CreatedAt

        CreateMap<Domain.Entities.Classification, ClassificationDto>();
    }
}
