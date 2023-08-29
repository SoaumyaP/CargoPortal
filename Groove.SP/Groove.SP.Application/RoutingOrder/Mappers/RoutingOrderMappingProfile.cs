using Groove.SP.Application.Mappers;
using Groove.SP.Application.RoutingOrder.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.RoutingOrder.Mappers
{
    public class RoutingOrderMappingProfile : MappingProfileBase<RoutingOrderModel, RoutingOrderViewModel>
    {
        public RoutingOrderMappingProfile()
        {
            CreateMap<RoutingOrderModel, RoutingOrderViewModel>()
                .ForMember(d => d.LastShipmentDate, opt => opt.MapFrom(s => s.LastDateForShipment))
                .ReverseMap();
        }
    }
}
