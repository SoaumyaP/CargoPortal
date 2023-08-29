using Groove.SP.Application.Mappers;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ShipmentLoads.Mappers
{
    public class ShipmentLoadMappingProfile : MappingProfileBase<ShipmentLoadModel, ShipmentLoadViewModel>
    {
        public ShipmentLoadMappingProfile()
        {
            CreateMap<ShipmentLoadModel, ShipmentLoadViewModel>()
                .ReverseMap();

            CreateMap<ShipmentLoadViewModel, ShipmentLoadModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }       
    }
}
