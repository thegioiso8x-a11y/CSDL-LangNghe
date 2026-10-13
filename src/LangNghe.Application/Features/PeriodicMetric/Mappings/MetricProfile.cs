using AutoMapper;
using LangNghe.Application.Features.PeriodicMetric.Dtos;
using LangNghe.Domain.Entities;

namespace LangNghe.Application.Features.PeriodicMetric.Mappings;

public class MetricProfile : Profile
{
    public MetricProfile()
    {
        CreateMap<CreateMetricDto, VillagePeriodicMetric>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.CommuneId, opt => opt.Ignore())
            .ForMember(x => x.ProvinceId, opt => opt.Ignore())
            .ForMember(x => x.ImportBatchId, opt => opt.Ignore())
            .ForMember(x => x.ExportValueVND, opt => opt.Ignore())
            .ForMember(x => x.IsEffective, opt => opt.Ignore())
            .ForMember(x => x.ImportedAt, opt => opt.Ignore());

        CreateMap<UpdateMetricDto, VillagePeriodicMetric>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.CraftVillageId, opt => opt.Ignore())
            .ForMember(x => x.CommuneId, opt => opt.Ignore())
            .ForMember(x => x.ProvinceId, opt => opt.Ignore())
            .ForMember(x => x.ReportPeriod, opt => opt.Ignore())
            .ForMember(x => x.PeriodType, opt => opt.Ignore())
            .ForMember(x => x.ImportBatchId, opt => opt.Ignore())
            .ForMember(x => x.ExportValueVND, opt => opt.Ignore())
            .ForMember(x => x.IsEffective, opt => opt.Ignore())
            .ForMember(x => x.ImportedAt, opt => opt.Ignore());
    }
}
