using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Permissions.ViewModels;
using Groove.SP.Application.Users.Validations;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Users.ViewModels
{
    public class UserProfileViewModel : ViewModelBase<UserProfileModel>
    {
        public long Id { get; set; }
        public string AccountNumber { get; set; }
        public string Username { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Department { get; set; }

        public string Phone { get; set; }

        public string CompanyName { get; set; }

        public string CompanyAddressLine1 { get; set; }

        public string CompanyAddressLine2 { get; set; }

        public string CompanyAddressLine3 { get; set; }

        public string CompanyAddressLine4 { get; set; }

        public string CompanyWeChatOrWhatsApp { get; set; }

        public string Customer { get; set; }

        public string ProfilePicture { get; set; }

        public UserStatus Status { get; set; }

        public DateTime LastSignInDate { get; set; }

        public string StatusName => EnumHelper<UserStatus>.GetDisplayName(this.Status);

        public bool IsInternal { get; set; }

        public long? CountryId { get; set; }

        public long? OrganizationId { get; set; }

        public long? OrganizationRoleId { get; set; }

        public string OrganizationName { get; set; }

        public string OrganizationCode { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public string OrganizationTypeName => EnumHelper<OrganizationType>.GetDisplayName(this.OrganizationType);

        public long? OPCountryId { get; set; }

        public string OPLocationName { get; set; }

        public string OPContactEmail { get; set; }

        public string OPContactName { get; set; }

        public string TaxpayerId { get; set; }

        public ICollection<UserRoleViewModel> UserRoles { get; set; }

        public RoleViewModel Role => UserRoles?.Select(ur => ur.Role).FirstOrDefault();

        public ICollection<PermissionViewModel> Permissions { get; set; }

        public string IdentityType { get; set; }

        public string IdentityTenant { get; set; }

        /// <summary>
        /// To define if current user is in switch mode (pretending to external user role)
        /// </summary>
        public bool IsUserRoleSwitch { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new UserProfileValidation().ValidateAndThrow(this);
        }        
        
    }
}
