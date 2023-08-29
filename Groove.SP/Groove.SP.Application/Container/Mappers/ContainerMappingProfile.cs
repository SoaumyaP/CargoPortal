using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Container.Mappers
{
    public class ContainerMappingProfile : MappingProfileBase<ContainerModel, ContainerViewModel>
    {
        public ContainerMappingProfile()
        {
            CreateMap<ContainerModel, ContainerViewModel>().ReverseMap().ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<ContainerModel, QuickTrackContainerViewModel>().ReverseMap();

            CreateMap<ContainerViewModel, ContainerModel>()
                .ForMember(src => src.Consolidation, opt => opt.Ignore())
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<UpdateContainerViaUIViewModel, ContainerModel>();
        }
    }
}
