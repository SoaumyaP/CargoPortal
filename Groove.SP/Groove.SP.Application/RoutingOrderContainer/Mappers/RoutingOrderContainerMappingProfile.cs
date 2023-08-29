using Groove.SP.Application.Mappers;
using Groove.SP.Application.RoutingOrderContainer.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.RoutingOrderContainer.Mappers
{
    public class RoutingOrderContainerMappingProfile : MappingProfileBase<RoutingOrderContainerModel, RoutingOrderContainerViewModel>
    {
        public RoutingOrderContainerMappingProfile()
        {
            CreateMap<RoutingOrderContainerModel, RoutingOrderContainerViewModel>().ReverseMap();
        }
    }
}
