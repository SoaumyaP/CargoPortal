using Groove.CSFE.Application.EventCodes.ViewModels;
using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Core.Entities;
namespace Groove.CSFE.Application.EventCodes.Mappers
{
    public class EventCodeMappingProfile : MappingProfileBase<EventCodeModel, EventCodeViewModel>
    {
        public EventCodeMappingProfile()
        {
            CreateMap<EventCodeModel, EventCodeViewModel>()
                .ForMember(dest => dest.ActivityType, src => src.MapFrom(ec => ec.ActivityType.Code))
                .ForMember(dest => dest.ActivityTypeDescription, src => src.MapFrom(ec => ec.ActivityType.Description))
                .ForMember(dest => dest.ActivityTypeLevel, src => src.MapFrom(ec => ec.ActivityType.EventLevel))
                .ForMember(dest => dest.ActivityTypeLevelDescription, src => src.MapFrom(ec => ec.ActivityType.LevelDescription))
                .ReverseMap();

            CreateMap<EventCodeModel, CreateEventCodeViewModel>().ReverseMap();
            CreateMap<UpdateEventSequenceViewModel, EventCodeModel>();
        }
    }
}
