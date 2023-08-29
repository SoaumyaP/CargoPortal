using Groove.CSFE.Application.Countries.ViewModels;
using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Application.Countries.Mappers
{
    public class CountryMappingProfile : MappingProfileBase<CountryModel, CountryViewModel>
    {
        public CountryMappingProfile()
        {
            CreateMap<CountryModel, CountryViewModel>().ReverseMap();

            CreateMap<CountryViewModel, CountryModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
