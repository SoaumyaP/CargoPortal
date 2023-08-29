using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Application.CruiseOrders.ViewModels;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using System;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.CruiseOrders.Mappers
{

    public class CruiseOrderMappingProfile : MappingProfileBase<CruiseOrderModel, CreateCruiseOrderViewModel>
    {
        public CruiseOrderMappingProfile()
        {
            CreateMap<CruiseOrderModel, CruiseOrderViewModel>()
               .ReverseMap();

            CreateMap<CreateCruiseOrderViewModel, CruiseOrderModel>();

            CreateMap<UpdateCruiseOrderViewModel, CruiseOrderModel>()
                .ForMember(d => d.POStatus, opt => opt.PreCondition(s => s.POStatus.HasValue))
                .ForAllMembers(opt => opt.PreCondition(src => src.IsPropertyDirty(opt.DestinationMember.Name)));

            CreateMap<ReviseCruiseOrderViewModel, CruiseOrderModel>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Items, opt => opt.Ignore())
                .ForMember(d => d.Contacts, opt => opt.Ignore());


            CreateMap<CruiseOrderContactModel, CruiseOrderContactViewModel>()
                .ReverseMap();

            CreateMap<CruiseOrderItemModel, CruiseOrderItemViewModel>()
                .ForMember(d => d.UOM, opt => opt.MapFrom(s => s.UOM.HasValue ? s.UOM.Value.ToString() : null))
                .ReverseMap()
                .ForMember(d => d.UOM, opt => opt.MapFrom(s => Enum.Parse(typeof(CruiseUOM), s.UOM, true)));
            
            CreateMap<ReviseCruiseOrderItemViewModel, CruiseOrderItemModel>()
                // Ignore some fields as not allowed to edit via UI
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.OrderId, opt => opt.Ignore())
                .ForMember(d => d.POLine, opt => opt.Ignore())
                .ForMember(d => d.ItemId, opt => opt.Ignore())
                .ForMember(d => d.ItemName, opt => opt.Ignore())
                .ForMember(d => d.LineEstimatedDeliveryDate, opt => opt.Ignore())
                .ForMember(d => d.FirstReceivedDate, opt => opt.Ignore())
                .ForMember(d => d.MakerReferenceOfItemName2, opt => opt.Ignore())
                .ForMember(d => d.RequestLineShoreNotes, opt => opt.Ignore())
                .ForMember(d => d.ShipRequestLineNotes, opt => opt.Ignore())
                .ForMember(d => d.QuantityDelivered, opt => opt.Ignore())
                .ForMember(d => d.RequestLine, opt => opt.Ignore())
                .ForMember(d => d.RequestNumber, opt => opt.Ignore())
                .ForMember(d => d.RequestQuantity, opt => opt.Ignore())
                .ForMember(d => d.UOM, opt => opt.Ignore())
                .ForMember(d => d.NetUSUnitPrice, opt => opt.Ignore())
                .ForMember(d => d.TotalOrderPrice, opt => opt.Ignore())
                .ForMember(d => d.OrderQuantity, opt => opt.Ignore())
                .ForMember(d => d.BuyerName, opt => opt.Ignore())
                .ForMember(d => d.Comments, opt => opt.Ignore())
                .ForMember(d => d.LatestDialog, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.CreatedDate, opt => opt.Ignore());


            CreateMap<CruiseOrderQueryModel, CruiseOrderListViewModel>()
                .ForMember(d => d.PODate, opt => opt.MapFrom(s => s.PODate.Value.Date));
        }
    }
}
