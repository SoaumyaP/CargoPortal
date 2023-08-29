using Groove.CSFE.Application.Countries.ViewModels;
using Groove.CSFE.Application.Locations.ViewModels;
using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Application.Locations.Mappers
{
    public class LocationMappingProfile : MappingProfileBase<LocationModel, LocationViewModel>
    {
        public LocationMappingProfile()
        {
            CreateMap<LocationModel, LocationViewModel>().ReverseMap();

            CreateMap<LocationViewModel, LocationModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
