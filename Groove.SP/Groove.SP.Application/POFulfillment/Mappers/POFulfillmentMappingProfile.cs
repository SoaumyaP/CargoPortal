using Groove.SP.Application.Mappers;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Linq;

namespace Groove.SP.Application.POFulfillment.Mappers
{
    public class POFulfillmentMappingProfile : MappingProfileBase<POFulfillmentModel, POFulfillmentViewModel>
    {
        public POFulfillmentMappingProfile()
        {
            CreateMap<POFulfillmentModel, POFulfillmentViewModel>().ReverseMap();
            CreateMap<InputPOFulfillmentViewModel, POFulfillmentModel>();
            CreateMap<POFulfillmentLoadViewModel, POFulfillmentLoadModel>().ReverseMap();
            CreateMap<POFulfillmentOrderViewModel, POFulfillmentOrderModel>().ReverseMap();
            CreateMap<POFulfillmentItineraryViewModel, POFulfillmentItineraryModel>().ReverseMap();
            CreateMap<POFulfillmentOrderModel, BookingRequestContentViewModel>();
            CreateMap<POFulfillmentModel, SummaryPOFulfillmentViewModel>()
                .ForMember(d => d.Shipments, opt => opt.Ignore())
                .ForMember(d => d.FulfillmentUnitQty, opt => opt.Ignore());

            CreateMap<POFulfillmentModel, SummaryBuyerApprovalPOFFViewModel>()
            .ForMember(x => x.PurchaseOrderNos, y => y.MapFrom(src => src.Orders.Select(z => new Tuple<long, string>(z.PurchaseOrderId, z.CustomerPONumber)).Distinct()));

            CreateMap<POFulfillmentLoadDetailModel, POFulfillmentLoadDetailViewModel>();

            CreateMap<POFulfillmentAllocatedOrderViewModel, POFulfillmentAllocatedOrderModel>();

            CreateMap<ImportPOFulfillmentCargoReceiveItemViewModel, POFulfillmentCargoReceiveItemModel>();

            CreateMap<ImportBookingOrderViewModel, POFulfillmentOrderModel>();

            CreateMap<EdiSonUpdateConfirmPOFFViewModel, POFulfillmentModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<EdiSonUpdateConfirmPOFFViewModel, POFulfillmentBookingRequestModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}