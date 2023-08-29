using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.Permissions.ViewModels
{
    public class PermissionViewModel : ViewModelBase<PermissionModel>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            
        }
    }
}
