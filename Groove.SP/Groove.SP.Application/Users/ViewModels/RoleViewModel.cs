using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Users.Validations;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.Users.ViewModels
{
    public class RoleViewModel : ViewModelBase<RoleModel>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Activated { get; set; }
        public bool IsInternal { get; set; }
        public RoleStatus Status { get; set; }
        public bool IsOfficial { get; set; }
        public string StatusName => EnumHelper<RoleStatus>.GetDisplayName(this.Status);
        //public ICollection<userro> UserProfileRoles { get; set; }
        public ICollection<long> PermissionIds { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new RoleValidation().ValidateAndThrow(this);
        }
    }
}
