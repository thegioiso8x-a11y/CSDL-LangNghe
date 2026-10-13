using LangNghe.Application.Common;
using AutoMapper;
using LangNghe.Application.Features.Workflow.Dtos;
using LangNghe.Domain.Entities;

namespace LangNghe.Application.Features.Workflow.Mappings;

public class WorkflowProfile : Profile
{
    public WorkflowProfile()
    {
        CreateMap<SubmissionWorkflow, SubmissionDto>()
            .ForMember(dest => dest.VillageName, opt => opt.MapFrom(src => src.CraftVillage != null ? src.CraftVillage.Name : null));
    }
}
