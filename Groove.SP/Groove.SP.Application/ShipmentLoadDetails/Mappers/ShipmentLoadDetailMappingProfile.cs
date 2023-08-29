using System;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Core.Entities;
using System.Linq;

namespace Groove.SP.Application.ShipmentLoadDetails.Mappers
{
    public class ShipmentLoadDetailMappingProfile : MappingProfileBase<ShipmentLoadDetailModel, ShipmentLoadDetailViewModel>
    {
        public ShipmentLoadDetailMappingProfile()
        {
            CreateMap<ShipmentLoadDetailModel, ShipmentLoadDetailViewModel>()
            .ForMember(x => x.BillOfLadingNos, y => y.MapFrom(src => src.Shipment.ShipmentBillOfLadings
                            .Select(z => new Tuple<long, string>(z.BillOfLadingId, z.BillOfLading.BillOfLadingNo))))
            .ReverseMap();

            CreateMap<ShipmentLoadDetailModel, ShipmentLoadDetailListViewModel>()
            .ForMember(x => x.BillOfLadingNos, y => y.MapFrom(src => src.Shipment.ShipmentBillOfLadings
                            .Select(z => new Tuple<long, string>(z.BillOfLadingId, z.BillOfLading.BillOfLadingNo))));

            CreateMap<ShipmentLoadDetailViewModel, ShipmentLoadDetailModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<CargoLoadDetailQueryModel, CargoLoadDetailViewModel>();

            CreateMap<UpdateShipmentLoadDetailViewModel, ShipmentLoadDetailModel>();

            CreateMap<ImportShipmentLoadDetailViewModel, ShipmentLoadDetailModel>();
        }
    }
}
