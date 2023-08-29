using Groove.SP.Application.BillOfLadingShipmentLoad.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLadingShipmentLoad.Mappers
{
    public class BillOfLadingShipmentLoadMappingProfile : MappingProfileBase<BillOfLadingShipmentLoadModel, BillOfLadingShipmentLoadViewModel>
    {
        public BillOfLadingShipmentLoadMappingProfile()
        {
            CreateMap<BillOfLadingShipmentLoadModel, BillOfLadingShipmentLoadViewModel>().ReverseMap().ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<BillOfLadingShipmentLoadViewModel, BillOfLadingShipmentLoadModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
