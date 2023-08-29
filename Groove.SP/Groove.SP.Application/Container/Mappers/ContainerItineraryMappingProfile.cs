using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Container.Mappers
{
    public class ContainerItineraryMappingProfile : MappingProfileBase<ContainerItineraryModel, ContainerItineraryViewModel>
    {
        public ContainerItineraryMappingProfile()
        {
            CreateMap<ContainerItineraryModel, ContainerItineraryViewModel>()
                .ReverseMap()
                .ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<ContainerItineraryViewModel, ContainerItineraryModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
