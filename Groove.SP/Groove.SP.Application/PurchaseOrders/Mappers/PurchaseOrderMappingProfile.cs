using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Models;
using System;
using System.Globalization;
using System.Linq;

namespace Groove.SP.Application.PurchaseOrders.Mappers{

    public class PurchaseOrderMappingProfile : MappingProfileBase<PurchaseOrderModel, PurchaseOrderViewModel>
    {
        public PurchaseOrderMappingProfile()
        {
            CreateMap<PurchaseOrderModel, PurchaseOrderViewModel>()
                .ForMember(d => d.Supplier, opt => opt.PreCondition(s => s.Contacts != null && s.Contacts.Any(a => a.OrganizationRole == OrganizationRole.Supplier)))
                .ForMember(d => d.Supplier, opt => opt.MapFrom(s => s.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Supplier).CompanyName))
                .ForMember(d => d.Shipper, opt => opt.PreCondition(s => s.Contacts != null && s.Contacts.Any(a => a.OrganizationRole == OrganizationRole.Shipper)))
                .ForMember(d => d.Shipper, opt => opt.MapFrom(s => s.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Shipper).CompanyName))
                .ForMember(d => d.Customer, opt => opt.PreCondition(s => s.Contacts != null && s.Contacts.Any(a => a.OrganizationRole == OrganizationRole.Principal)))
                .ForMember(d => d.Customer, opt => opt.MapFrom(s => s.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Principal).CompanyName))
                .ForMember(d => d.Consignee, opt => opt.PreCondition(s => s.Contacts != null && s.Contacts.Any(a => a.OrganizationRole == OrganizationRole.Consignee)))
                .ForMember(d => d.Consignee, opt => opt.MapFrom(s => s.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee).CompanyName))
                .ReverseMap();

            CreateMap<PurchaseOrderModel, ViewAllocatedPOViewModel>();

            CreateMap<ExcelPOViewModel, PurchaseOrderModel>()
                .ForMember(d => d.Stage, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.RowVersion, opt => opt.Ignore());

            CreateMap<ExcelPOContactViewModel, PurchaseOrderContactModel>()
                .ForMember(d => d.OrganizationRole, opt => opt.MapFrom(
                    s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.OrganizationRole.ToLower())));

            CreateMap<ExcelPOLineItemViewModel, POLineItemModel>()
                .ForMember(d => d.POLineKey, opt => opt.MapFrom(s => s.ProductCode))
                .ForMember(d => d.UnitUOM, opt => opt.MapFrom(s => Enum.Parse(typeof(UnitUOMType), s.UnitUOM, true)))
                .ForMember(d => d.PackageUOM, opt => opt.PreCondition(s => !string.IsNullOrWhiteSpace(s.PackageUOM)))
                .ForMember(d => d.PackageUOM, opt => opt.MapFrom(s => Enum.Parse(typeof(PackageUOMType), s.PackageUOM, true)));

            CreateMap<ExcelPOLineItemViewModel, POLineItemViewModel>()
                .ForMember(d => d.POLineKey, opt => opt.MapFrom(s => s.ProductCode));

            CreateMap<CreatePOViewModel, PurchaseOrderModel>()
                .ForMember(d => d.Stage, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ForMember(d => d.Incoterm, opt => opt.PreCondition(a => !string.IsNullOrWhiteSpace(a.Incoterm)))
                .ForMember(d => d.Incoterm, opt => opt.MapFrom(s => Enum.Parse(typeof(IncotermType), s.Incoterm, true)))
                .ForMember(d => d.ModeOfTransport, opt => opt.PreCondition(s => !string.IsNullOrWhiteSpace(s.ModeOfTransport)))
                .ForMember(d => d.ModeOfTransport, opt => opt.MapFrom(s => Enum.Parse(typeof(ModeOfTransportType), s.ModeOfTransport, true)));

            CreateMap<UpdatePOViewModel, PurchaseOrderModel>()
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ForMember(d => d.CargoReadyDate, opt => opt.PreCondition(s => s.CargoReadyDate.HasValue))
                .ForMember(d => d.Status, opt => opt.PreCondition(s => s.Status.HasValue))
                .ForMember(d => d.Stage, opt => opt.PreCondition(s => s.Stage.HasValue))
                .ForMember(d => d.LineItems, opt => opt.Ignore())
                .ForMember(d => d.Contacts, opt => opt.PreCondition(s => s.Contacts?.Count > 0))
                .ForMember(d => d.Incoterm, opt => opt.
                    MapFrom(s => string.IsNullOrWhiteSpace(s.Incoterm) ? null 
                    : Enum.Parse(typeof(IncotermType), s.Incoterm, true)))
                .ForMember(d => d.ModeOfTransport, opt => opt.
                    MapFrom(s => string.IsNullOrWhiteSpace(s.ModeOfTransport) ? null 
                    : Enum.Parse(typeof(ModeOfTransportType), s.ModeOfTransport, true)))
                .ForAllMembers(opt => opt.Condition(src => src.IsPropertyDirty(opt.DestinationMember.Name)));

            CreateMap<CreateOrUpdatePOContactViewModel, PurchaseOrderContactModel>()
                .ForMember(d => d.OrganizationCode, opt => opt.MapFrom(s => s.OrganizationCode.Trim()))
                .ForMember(d => d.OrganizationRole, opt => opt.MapFrom(
                    s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.OrganizationRole.ToLower())))
                .ForAllMembers(opt => opt.Condition(s => s.IsPropertyDirty(opt.DestinationMember.Name)));
                

            CreateMap<POLineItemViewModel, POLineItemModel>()
                .ReverseMap();

            CreateMap<PurchaseOrderModel, BookingPOViewModel>();
            CreateMap<POLineItemModel, BookingPOLineItemViewModel>()
                .ForMember(d => d.OuterDepth, opt => opt.Ignore())
                .ForMember(d => d.OuterHeight, opt => opt.Ignore())
                .ForMember(d => d.OuterQuantity, opt => opt.Ignore())
                .ForMember(d => d.InnerQuantity, opt => opt.Ignore())
                .ForMember(d => d.OuterWidth, opt => opt.Ignore())
                .ForMember(d => d.OuterGrossWeight, opt => opt.Ignore());
            CreateMap<POLineItemModel, ShipmentPOLineItemViewModel>();

            CreateMap<PurchaseOrderAdhocChangeModel, PurchaseOrderAdhocChangeViewModel>().ReverseMap();

            CreateMap<PurchaseOrderQueryModel, PurchaseOrderListViewModel>()
                .ForMember(d => d.CreatedDate, opt => opt.MapFrom(s => s.CreatedDate.Date))
                .ForMember(d => d.StageName, opt => opt.Ignore())
                .ForMember(d => d.StatusName, opt => opt.Ignore());

            CreateMap<PurchaseOrderProgressCheckViewModel, PurchaseOrderModel>()
                .ForAllMembers(opt => opt.Condition(src => src.IsPropertyDirty(opt.DestinationMember.Name)));

            CreateMap<ExcelPOViewModel, POViewModel>();
            CreateMap<CreatePOViewModel, POViewModel>();
            CreateMap<UpdatePOViewModel, POViewModel>();
            CreateMap<PurchaseOrderSingleQueryModel, PurchaseOrderQueryModel>();
        }
    }
}
