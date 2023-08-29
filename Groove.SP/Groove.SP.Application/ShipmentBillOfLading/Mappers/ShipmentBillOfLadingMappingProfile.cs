using Groove.SP.Application.Mappers;
using Groove.SP.Application.ShipmentBillOfLading.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ShipmentBillOfLading.Mappers
{
    public class ShipmentBillOfLadingMappingProfile : MappingProfileBase<ShipmentBillOfLadingModel, ShipmentBillOfLadingViewModel>
    {
        public ShipmentBillOfLadingMappingProfile()
        {
            CreateMap<ShipmentBillOfLadingModel, ShipmentBillOfLadingViewModel>().ReverseMap().ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<ShipmentBillOfLadingViewModel, ShipmentBillOfLadingModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
