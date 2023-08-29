using Groove.CSFE.Application.Countries.ViewModels;
using Groove.CSFE.Application.Locations.ViewModels;
using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Groove.CSFE.Application.AlternativeLocations.ViewModels;

namespace Groove.CSFE.Application.AlternativeLocations.Mappers
{
    public class AlternativeLocationMappingProfile : MappingProfileBase<AlternativeLocationModel, AlternativeLocationViewModel>
    {
        public AlternativeLocationMappingProfile()
        {
            CreateMap<AlternativeLocationModel, AlternativeLocationViewModel>().ReverseMap();

            CreateMap<AlternativeLocationViewModel, AlternativeLocationModel>();
        }
    }
}
