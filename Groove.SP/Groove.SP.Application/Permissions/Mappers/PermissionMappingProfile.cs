using Groove.SP.Application.Mappers;
using Groove.SP.Application.Permissions.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.Permissions.Mappers
{
    public class PermissionMappingProfile : MappingProfileBase<PermissionModel, PermissionViewModel>
    {
        public PermissionMappingProfile()
        {
            CreateMap<PermissionModel, PermissionViewModel>();
        }
    }
}
