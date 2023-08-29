using Groove.SP.Application.FreightScheduler.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.FreightScheduler.Mappers
{
    public class FreightSchedulerMappingProfile : MappingProfileBase<FreightSchedulerModel, FreightSchedulerViewModel>
    {
        public FreightSchedulerMappingProfile()
        {
            CreateMap<FreightSchedulerModel, FreightSchedulerViewModel>();

            CreateMap<FreightSchedulerViewModel, FreightSchedulerModel>()
                .ForAllMembers(dest => dest.Condition(src => src.IsPropertyDirty(dest.DestinationMember.Name)));

            CreateMap<UpdateFreightSchedulerViewModel, FreightSchedulerModel>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedDate, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.UpdatedDate, opt => opt.Ignore())
                .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
                .ForMember(d => d.ModeOfTransport, opt => opt.Ignore())
                .ForMember(d => d.CarrierCode, opt => opt.Ignore())
                .ForMember(d => d.CarrierName, opt => opt.Ignore())
                .ForMember(d => d.VesselName, opt => opt.Ignore())
                .ForMember(d => d.Voyage, opt => opt.Ignore())
                .ForMember(d => d.MAWB, opt => opt.Ignore())
                .ForMember(d => d.LocationFromCode, opt => opt.Ignore())
                .ForMember(d => d.LocationFromName, opt => opt.Ignore())
                .ForMember(d => d.LocationToCode, opt => opt.Ignore())
                .ForMember(d => d.LocationToName, opt => opt.Ignore());
        }
    }
}
