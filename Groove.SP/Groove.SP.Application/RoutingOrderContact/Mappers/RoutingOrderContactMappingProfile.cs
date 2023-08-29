using Groove.SP.Application.Mappers;
using Groove.SP.Application.RoutingOrderContact.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.RoutingOrderContact.Mappers
{
    public class RoutingOrderContactMappingProfile : MappingProfileBase<RoutingOrderContactModel, RoutingOrderContactViewModel>
    {
        public RoutingOrderContactMappingProfile()
        {
            CreateMap<RoutingOrderContactModel, RoutingOrderContactViewModel>().ReverseMap();
            CreateMap<ImportRoutingOrderContactViewModel, RoutingOrderContactModel>();
        }
    }
}
