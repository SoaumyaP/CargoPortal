using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Activity.Mappers
{
    public class ActivityMappingProfile : MappingProfileBase<ActivityModel, ActivityViewModel>
    {
        public ActivityMappingProfile()
        {
            CreateMap<ActivityModel, ActivityViewModel>()
                .ReverseMap();

            CreateMap<ActivityViewModel, ActivityModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<AgentActivityUpdateViewModel, ActivityModel>()
                .ForMember(x => x.GlobalIdActivities, x => x.Ignore())
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
