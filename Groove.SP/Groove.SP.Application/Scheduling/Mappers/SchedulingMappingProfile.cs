using Groove.SP.Application.Mappers;
using Groove.SP.Application.Scheduling.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Scheduling.Mappers
{
    public class SchedulingMappingProfile : MappingProfileBase<SchedulingModel, SchedulingViewModel>
    {
        public SchedulingMappingProfile()
        {
            CreateMap<SchedulingModel, SchedulingListViewModel>();

            CreateMap<SchedulingQueryModel, SchedulingViewModel>();


            CreateMap<SchedulingModel, SchedulingViewModel>();
            CreateMap<SchedulingViewModel, SchedulingModel>()
                .ForMember(d => d.CSPortalReportId, s => s.MapFrom(x => x.CSPortalReportId))
                .ForMember(d => d.Status, s => s.MapFrom(x => x.Enabled ? SchedulingStatus.Active : SchedulingStatus.Inactive));


            CreateMap<SchedulingViewModel, TelerikTaskModel>()
                .ForMember(d => d.NextOccurence, s => s.MapFrom(x => x.NextOccurrence))
                .ForMember(d => d.Id, s => s.MapFrom(x => x.TelerikSchedulingId));

            CreateMap<TelerikTaskModel, SchedulingViewModel>()
                .ForMember(d => d.TelerikSchedulingId, s => s.MapFrom(x => x.Id))
                .ForMember(d => d.NextOccurrence, s => s.MapFrom(x => x.NextOccurence))
                .ForMember(d => d.Id, s => s.Ignore());
        }
    }
}
