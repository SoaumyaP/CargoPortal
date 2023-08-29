using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Groove.SP.Application.GlobalIdActivity.ViewModels;

namespace Groove.SP.Application.GlobalIdActivity.Mappers
{
    public class GlobalIdActivityMappingProfile : MappingProfileBase<GlobalIdActivityModel, GlobalIdActivityViewModel>
    {
        public GlobalIdActivityMappingProfile()
        {
            CreateMap<GlobalIdActivityModel, GlobalIdActivityViewModel>()
                .ReverseMap();
        }
    }
}
