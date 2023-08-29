using System;

using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Users.ViewModels
{
    public class UserRoleViewModel : ViewModelBase<UserRoleModel>
    {
        public long UserId { get; set; }

        public long RoleId { get; set; }

        public RoleViewModel Role { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }
}
