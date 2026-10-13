using AutoMapper;
using LangNghe.Application.Features.OutputUnit.Dtos;

namespace LangNghe.Application.Features.OutputUnit.Mappings;

public class OutputUnitProfile : Profile
{
    public OutputUnitProfile()
    {
        CreateMap<CreateOutputUnitDto, Domain.Entities.OutputUnit>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateOutputUnitDto, Domain.Entities.OutputUnit>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<Domain.Entities.OutputUnit, OutputUnitDto>();
    }
}
