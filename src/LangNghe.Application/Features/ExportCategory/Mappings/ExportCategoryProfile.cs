using AutoMapper;
using LangNghe.Application.Features.ExportCategory.Dtos;

namespace LangNghe.Application.Features.ExportCategory.Mappings;

public class ExportCategoryProfile : Profile
{
    public ExportCategoryProfile()
    {
        CreateMap<CreateExportCategoryDto, Domain.Entities.ExportCategory>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateExportCategoryDto, Domain.Entities.ExportCategory>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<Domain.Entities.ExportCategory, ExportCategoryDto>();
    }
}
