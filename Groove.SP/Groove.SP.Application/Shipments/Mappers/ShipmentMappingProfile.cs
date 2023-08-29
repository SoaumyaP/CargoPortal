using System;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities;
using System.Linq;
using Groove.SP.Core.Models;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.QuickTrack;

namespace Groove.SP.Application.Shipments.Mappers
{
    public class ShipmentMappingProfile : MappingProfileBase<ShipmentModel, ShipmentViewModel>
    {
        public ShipmentMappingProfile()
        {
            CreateMap<ShipmentModel, ShipmentViewModel>()
                .ForMember(d => d.FulfillmentId, opt => opt.PreCondition(s => s.POFulfillment != null))
                .ForMember(d => d.FulfillmentId, d => d.MapFrom(s => s.POFulfillment.Id))
                .ForMember(d => d.FulfillmentNumber, opt => opt.PreCondition(s => s.POFulfillment != null))
                .ForMember(d => d.FulfillmentNumber, d => d.MapFrom(s => s.POFulfillment.Number))
                .ForMember(d => d.FulfillmentStage, opt => opt.PreCondition(s => s.POFulfillment != null))
                .ForMember(d => d.FulfillmentStage, d => d.MapFrom(s => s.POFulfillment.Stage))
                .ForMember(d => d.FulfillmentType, opt => opt.PreCondition(s => s.POFulfillment != null))
                .ForMember(d => d.FulfillmentType, d => d.MapFrom(s => s.POFulfillment.FulfillmentType))
                .ForMember(d => d.OrderType, d => d.MapFrom(s => (int)s.OrderType))
                .ForMember(d => d.CYClosingDate, d => d.MapFrom(s => s.POFulfillment.CYClosingDate))
                .ForMember(d => d.CFSClosingDate, d => d.MapFrom(s => s.POFulfillment.CFSClosingDate))
                .ForMember(d => d.CYEmptyPickupTerminalCode, d => d.MapFrom(s => s.POFulfillment.CYEmptyPickupTerminalCode))
                .ForMember(d => d.CFSWarehouseCode, d => d.MapFrom(s => s.POFulfillment.CFSWarehouseCode))
                .ForMember(d => d.CYEmptyPickupTerminalDescription, d => d.MapFrom(s => s.POFulfillment.CYEmptyPickupTerminalDescription))
                .ForMember(d => d.CFSWarehouseDescription, d => d.MapFrom(s => s.POFulfillment.CFSWarehouseDescription))
                .ForMember(v => v.BillOfLadingNos,
                    c => c.MapFrom(m =>
                        m.ShipmentBillOfLadings
                            .Where(x => x.BillOfLading != null)
                            .Select(sb => new Tuple<long, string>(sb.BillOfLadingId, sb.BillOfLading.BillOfLadingNo))))
                .ForMember(v => v.MasterBillNos,
                            c => c.MapFrom(m =>
                            m.Consignments.Where(x => x.MasterBill != null)
                                .Select(csm => new Tuple<long, string>(
                                    csm.MasterBill.Id,
                                    csm.MasterBill.ModeOfTransport != null && csm.MasterBill.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase) ? csm.MasterBill.MasterBillOfLadingNo.Insert(3, "-") : csm.MasterBill.MasterBillOfLadingNo))
                                .Distinct())
                );

            CreateMap<ShipmentModel, QuickTrackShipmentViewModel>()
                .ForMember(v => v.BillOfLadingNos, c => c.MapFrom(m => m.ShipmentBillOfLadings.Select(sb => sb.BillOfLading.BillOfLadingNo)))
                .ReverseMap();

            CreateMap<ShipmentViewModel, ShipmentModel>()
                .ForMember(d => d.Id, s => s.Ignore())
                .ForMember(d => d.POFulfillmentId, d => d.PreCondition(s => s.FulfillmentId != null))
                .ForMember(d => d.POFulfillmentId, d => d.MapFrom(s => s.FulfillmentId))
                .ForMember(d => d.OrderType, d => d.PreCondition(s => !string.IsNullOrEmpty(s.OrderType)))
                .ForMember(d => d.OrderType, opt => opt.MapFrom(s => Enum.Parse<OrderType>(s.OrderType, true)))
                .ForAllOtherMembers(x => x.PreCondition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<ShipmentItemModel, ShipmentItemViewModel>()
                .ForMember(d => d.OuterQuantity, opt => opt.Ignore())
                .ForMember(d => d.InnerQuantity, opt => opt.Ignore());

            CreateMap<ShipmentModel, ShipmentBookingReferenceViewModel>();
            CreateMap<ShipmentModel, CruiseOrderItemShipmentReferenceViewModel>();

            CreateMap<UpdateShipmentViewModel, ShipmentModel>()
               .ForMember(d => d.CreatedDate, s => s.Ignore())
               .ForMember(d => d.CreatedBy, s => s.Ignore())
               .ForAllOtherMembers(opt => opt.PreCondition(src => src.IsPropertyDirty(opt.DestinationMember.Name)));

            CreateMap<ShipmentModel, SimpleShipmentViewModel>();

            CreateMap<ActivityViewModel, QuickTrackActivityViewModel>();
            CreateMap<ShipmentModel, QuickTrackShipmentViewModel>();

            CreateMap<ShipmentMilestoneSingleQueryModel, ShipmentQueryModel>();

        }
    }
}
