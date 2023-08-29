using System;
using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Users.Validations;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Users.ViewModels
{
    public class UserRequestViewModel : ViewModelBase<UserRequestModel>
    {
        public string Username { get; set; }

        public long Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string CompanyName { get; set; }

        public string CompanyAddressLine1 { get; set; }

        public string CompanyAddressLine2 { get; set; }

        public string CompanyAddressLine3 { get; set; }

        public string CompanyAddressLine4 { get; set; }

        public string CompanyWeChatOrWhatsApp { get; set; }

        public string Customer { get; set; }

        public UserStatus Status { get; set; }

        public string StatusName => EnumHelper<UserStatus>.GetDisplayName(this.Status);

        public bool IsInternal { get; set; }

        public string UserType => IsInternal ? "label.internalUser" : "lablel.externalUser";

        public DateTime CreatedDateOnly => CreatedDate.Date;

        public long? OrganizationId { get; set; }

        public long? OrganizationRoleId { get; set; }

        public string OrganizationName { get; set; }

        public string OrganizationCode { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public long? OPCountryId { get; set; }

        public string OPLocationName { get; set; }

        public string OPContactEmail { get; set; }

        public string OPContactName { get; set; }

        public string TaxpayerId { get; set; }

        public long? RoleId { get; set; }

        public string RoleName { get; set; }

        public UserRequestViewModel()
        : base()
        { }

        public UserRequestViewModel(UserRequestModel model) : base(model)
        {
            Username = model.Username;
            Email = model.Email;
            Status = model.Status;
            Name = model.Name;
            IsInternal = model.IsInternal;
            Phone = model.Phone;
            CompanyName = model.CompanyName;
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new UserRequestsValidation().ValidateAndThrow(this);
        }
    }
}
