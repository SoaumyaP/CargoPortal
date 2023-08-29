using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.BuyerCompliance.Mappers
{
    public class BuyerComplianceMappingProfile : MappingProfileBase<BuyerComplianceModel, BuyerComplianceViewModel>
    {
        public BuyerComplianceMappingProfile()
        {
            CreateMap<BuyerComplianceModel, BuyerComplianceViewModel>()
                .ForMember(d => d.HSCodeShipFromCountryIds, opt => opt.MapFrom(s => StringHelper.SplitToLong(s.HSCodeShipFromCountryIds, ',')))
                .ForMember(d => d.HSCodeShipToCountryIds, opt => opt.MapFrom(s => StringHelper.SplitToLong(s.HSCodeShipToCountryIds, ',')))
                .ForMember(d => d.IsProgressCargoReadyDates, opt => opt.MapFrom(s => s.IsProgressCargoReadyDate))
                .ReverseMap()
                .ForMember(d => d.HSCodeShipFromCountryIds, opt => opt.MapFrom(s => string.Join(",", s.HSCodeShipFromCountryIds)))
                .ForMember(d => d.HSCodeShipToCountryIds, opt => opt.MapFrom(s => string.Join(",", s.HSCodeShipToCountryIds)));
            CreateMap<SaveBuyerComplianceViewModel, BuyerComplianceModel>()
                .ForMember(x => x.CreatedDate, opt => opt.Condition(src => src.Id == 0))
                .ForMember(d => d.HSCodeShipFromCountryIds, opt => opt.MapFrom(s => string.Join(",", s.HSCodeShipFromCountryIds)))
                .ForMember(d => d.IsProgressCargoReadyDate, opt => opt.MapFrom(s => s.IsProgressCargoReadyDates))
                .ForMember(d => d.HSCodeShipToCountryIds, opt => opt.MapFrom(s => string.Join(",", s.HSCodeShipToCountryIds)));

            CreateMap<ActivateBuyerComplianceViewModel, BuyerComplianceModel>();
            CreateMap<PurchaseOrderVerificationSettingViewModel, PurchaseOrderVerificationSettingModel>().ReverseMap();
            CreateMap<ProductVerificationSettingViewModel, ProductVerificationSettingModel>().ReverseMap();
            CreateMap<BookingTimelessViewModel, BookingTimelessModel>().ReverseMap();
            CreateMap<CargoLoadabilityViewModel, CargoLoadabilityModel>().ForMember(d => d.RowVersion, opt => opt.Ignore()).ReverseMap();
            CreateMap<ShippingComplianceViewModel, ShippingComplianceModel>().ReverseMap();

            CreateMap<BookingPolicyViewModel, BookingPolicyModel>()
            .ForMember(d => d.RowVersion, opt => opt.Ignore())
            .ForMember(d => d.ModeOfTransports, opt => opt.MapFrom(s => EnumHelper<ModeOfTransportType>.ConvertToInt(s.ModeOfTransportIds)))
            .ForMember(d => d.IncotermSelections, dest => dest.MapFrom(s => EnumHelper<IncotermType>.ConvertToInt(s.IncotermTypeIds)))
            .ForMember(d => d.FulfillmentAccuracies, dest => dest.MapFrom(s => EnumHelper<FulfillmentAccuracyType>.ConvertToInt(s.FulfillmentAccuracyIds)))
            .ForMember(d => d.BookingTimeless, dest => dest.MapFrom(s => EnumHelper<BookingTimelessType>.ConvertToInt(s.BookingTimelessIds)))
            .ForMember(d => d.LogisticsServiceSelections, dest => dest.MapFrom(s => EnumHelper<LogisticsServiceType>.ConvertToInt(s.LogisticsServiceSelectionIds)))
            .ForMember(d => d.MovementTypeSelections, dest => dest.MapFrom(s => EnumHelper<MovementType>.ConvertToInt(s.MovementTypeIds)))
            .ForMember(d => d.CargoLoadabilities, dest => dest.MapFrom(s => EnumHelper<CargoLoadabilityType>.ConvertToInt(s.CargoLoadabilityIds)))
            .ForMember(d => d.ShipFromLocationSelections, opt => opt.MapFrom(s => string.Join(",", s.ShipFromIds)))
            .ForMember(d => d.ShipToLocationSelections, opt => opt.MapFrom(s => string.Join(",", s.ShipToIds)))
            .ForMember(d => d.CarrierSelections, dest => dest.MapFrom(s => string.Join(",", s.CarrierIds)))
            .ReverseMap()
            .ForMember(d => d.ModeOfTransportIds, opt => opt.MapFrom(s => EnumHelper<ModeOfTransportType>.ConvertToIds(s.ModeOfTransports)))
            .ForMember(d => d.FulfillmentAccuracyIds, opt => opt.MapFrom(s => EnumHelper<FulfillmentAccuracyType>.ConvertToIds(s.FulfillmentAccuracies)))
            .ForMember(d => d.IncotermTypeIds, opt => opt.MapFrom(s => EnumHelper<IncotermType>.ConvertToIds(s.IncotermSelections)))
            .ForMember(d => d.MovementTypeIds, opt => opt.MapFrom(s => EnumHelper<MovementType>.ConvertToIds(s.MovementTypeSelections)))
            .ForMember(d => d.BookingTimelessIds, opt => opt.MapFrom(s => EnumHelper<BookingTimelessType>.ConvertToIds(s.BookingTimeless)))
            .ForMember(d => d.LogisticsServiceSelectionIds, opt => opt.MapFrom(s => EnumHelper<LogisticsServiceType>.ConvertToIds(s.LogisticsServiceSelections)))
            .ForMember(d => d.CargoLoadabilityIds, opt => opt.MapFrom(s => EnumHelper<CargoLoadabilityType>.ConvertToIds(s.CargoLoadabilities)))
            .ForMember(d => d.ShipFromIds, opt => opt.MapFrom(s => StringHelper.Split(s.ShipFromLocationSelections, ',')))
            .ForMember(d => d.ShipToIds, opt => opt.MapFrom(s => StringHelper.Split(s.ShipToLocationSelections, ',')))
            .ForMember(d => d.CarrierIds, opt => opt.MapFrom(s => StringHelper.Split(s.CarrierSelections, ',')));

            CreateMap<AgentAssignmentViewModel, AgentAssignmentModel>()
            .ForMember(d => d.RowVersion, opt => opt.Ignore())
            .ForMember(d => d.PortSelectionIds, opt => opt.MapFrom(s => string.Join(",", s.PortSelectionIds)))
            .ReverseMap()
            .ForMember(d => d.PortSelectionIds, opt => opt.MapFrom(s => StringHelper.Split(s.PortSelectionIds, ',')));

            CreateMap<ComplianceSelectionViewModel, ComplianceSelectionModel>()
            .ForMember(d => d.ModeOfTransportSelections, opt => opt.MapFrom(s => EnumHelper<ModeOfTransportType>.ConvertToInt(s.ModeOfTransportIds)))
            .ForMember(d => d.CommoditySelections, opt => opt.MapFrom(s => EnumHelper<CommodityType>.ConvertToInt(s.CommodityIds)))
            .ForMember(d => d.IncotermSelections, opt => opt.MapFrom(s => EnumHelper<IncotermType>.ConvertToInt(s.IncotermTypeIds)))
            .ForMember(d => d.MovementTypeSelections, opt => opt.MapFrom(s => EnumHelper<MovementType>.ConvertToInt(s.MovementTypeIds)))
            .ForMember(d => d.LogisticsServiceSelections, opt => opt.MapFrom(s => EnumHelper<LogisticsServiceType>.ConvertToInt(s.LogisticsServiceSelectionIds)))
            .ForMember(d => d.ShipFromLocationSelections, opt => opt.MapFrom(s => string.Join(",", s.ShipFromLocationIds)))
            .ForMember(d => d.ShipToLocationSelections, opt => opt.MapFrom(s => string.Join(",", s.ShipToLocationIds)))
            .ForMember(d => d.CarrierSelections, opt => opt.MapFrom(s => string.Join(",", s.CarrierIds)))
            .ReverseMap()
            .ForMember(d => d.ModeOfTransportIds, opt => opt.MapFrom(s => EnumHelper<ModeOfTransportType>.ConvertToIds(s.ModeOfTransportSelections)))
            .ForMember(d => d.CommodityIds, opt => opt.MapFrom(s => EnumHelper<CommodityType>.ConvertToIds(s.CommoditySelections)))
            .ForMember(d => d.IncotermTypeIds, opt => opt.MapFrom(s => EnumHelper<IncotermType>.ConvertToIds(s.IncotermSelections)))
            .ForMember(d => d.MovementTypeIds, opt => opt.MapFrom(s => EnumHelper<MovementType>.ConvertToIds(s.MovementTypeSelections)))
            .ForMember(d => d.LogisticsServiceSelectionIds, opt => opt.MapFrom(s => EnumHelper<LogisticsServiceType>.ConvertToIds(s.LogisticsServiceSelections)))
            .ForMember(d => d.ShipFromLocationIds, opt => opt.MapFrom(s => StringHelper.Split(s.ShipFromLocationSelections, ',')))
            .ForMember(d => d.ShipToLocationIds, opt => opt.MapFrom(s => StringHelper.Split(s.ShipToLocationSelections, ',')))
            .ForMember(d => d.CarrierIds, opt => opt.MapFrom(s => StringHelper.Split(s.CarrierSelections, ',')));

            CreateMap<BuyerComplianceQueryModel, BuyerComplianceListViewModel>();

                
        }
    }
}