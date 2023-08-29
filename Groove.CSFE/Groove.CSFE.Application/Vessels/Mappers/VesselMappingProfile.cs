using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Application.Vessels.ViewModels;
using Groove.CSFE.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Application.Vessels.Mappers
{
    public class VesselMappingProfile : MappingProfileBase<VesselModel, VesselViewModel>
    {
        public VesselMappingProfile()
        {
            CreateMap<VesselModel, VesselViewModel>();

            CreateMap<VesselViewModel, VesselModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<CreateVesselViewModel, VesselModel>();

            CreateMap<UpdateVesselViewModel, VesselModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<VesselQueryModel, VesselListViewModel>();
        }
    }
}
