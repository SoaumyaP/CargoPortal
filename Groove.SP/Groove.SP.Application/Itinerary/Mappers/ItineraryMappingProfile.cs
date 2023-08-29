using System;
using System.Linq;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Itinerary.Mappers
{
    public class ItineraryMappingProfile : MappingProfileBase<ItineraryModel, ItineraryViewModel>
    {
        public ItineraryMappingProfile()
        {
            CreateMap<ItineraryModel, ItineraryViewModel>()
                .ForMember(dest => dest.ConsignmentId, 
                    opt => opt.PreCondition(src => src.ConsignmentItineraries != null))
                .ForMember(dest => dest.ConsignmentId, 
                    opt => opt.MapFrom(src => src.ConsignmentItineraries.First().ConsignmentId))
                .ReverseMap()
                .ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<ItineraryViewModel, ItineraryModel>();
            CreateMap<UpdateItineraryViewModel, ItineraryModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
            CreateMap<CreateItineraryViewModel, ItineraryModel>();

            CreateMap<ImportItineraryViewModel, ItineraryModel>()
                .ForMember(dest => dest.SCAC, opt => opt.MapFrom(src => src.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.OrdinalIgnoreCase) ? src.CarrierCode : null))
                .ForMember(dest => dest.AirlineCode, opt => opt.MapFrom(src => src.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.OrdinalIgnoreCase) ? src.CarrierCode : null))
                .ForMember(dest => dest.VesselFlight, opt => opt.MapFrom(
                    src => src.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.OrdinalIgnoreCase) ? $"{src.VesselName}/{src.Voyage}" : (src.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.OrdinalIgnoreCase) ? src.FlightNumber : null)));
        }
    }
}
