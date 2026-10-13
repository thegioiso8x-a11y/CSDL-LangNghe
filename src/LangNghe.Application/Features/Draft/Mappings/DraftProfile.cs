using LangNghe.Application.Common;
using AutoMapper;
using LangNghe.Application.Features.Draft.Dtos;
using LangNghe.Domain.Entities;

namespace LangNghe.Application.Features.Draft.Mappings;

public class DraftProfile : Profile
{
    public DraftProfile()
    {
        CreateMap<VillageDraft, DraftDto>();
    }
}
