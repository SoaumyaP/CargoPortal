using FluentValidation;
using Groove.SP.Application.POFulfillmentContact.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillmentContact.Validations
{
    public class ImportBookingContactValidator : AbstractValidator<ImportBookingContactViewModel>
    {
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly List<string> organizationRoles = new List<string> {
            OrganizationRole.Shipper,
            OrganizationRole.Consignee,
            OrganizationRole.Supplier,
            OrganizationRole.Principal,
            OrganizationRole.OriginAgent,
            OrganizationRole.DestinationAgent,
            OrganizationRole.NotifyParty,
            OrganizationRole.AlsoNotify,
            OrganizationRole.Pickup,
            OrganizationRole.BillingParty
        };
        public ImportBookingContactValidator(ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;

            RuleFor(a => a.OrganizationRole).NotEmpty();
            RuleFor(a => a.OrganizationRole)
                .Must(a => organizationRoles.Contains(a))
                .WithMessage($"'{nameof(ImportBookingContactViewModel.OrganizationRole)}' is an invalid role.")
                .When(a => !string.IsNullOrEmpty(a.OrganizationRole));

            RuleFor(a => a.OrganizationCode).NotEmpty();
            RuleFor(a => a.OrganizationCode)
                .MustAsync(async (organizationCode, cancellation) => await IsExistOrganizationByCodeAsync(organizationCode))
                .WithMessage($"'{nameof(ImportBookingContactViewModel.OrganizationCode)}' is not existing on master data, must be organization code.")
                .When(a => !string.IsNullOrEmpty(a.OrganizationCode));

            RuleFor(a => a.CompanyName)
                .NotEmpty()
                .When(a => a.OrganizationCode == "0");

            RuleFor(a => a.AddressLine1)
                .NotEmpty()
                .When(a => a.OrganizationCode == "0");

            RuleFor(a => a.ContactName)
                .NotEmpty()
                .When(a => a.OrganizationCode == "0");

            RuleFor(a => a.ContactEmail)
                .NotEmpty()
                .When(a => a.OrganizationCode == "0");
        }

        private async Task<bool> IsExistOrganizationByCodeAsync(string orgCode)
        {
            if (orgCode == "0")
            {
                return true;
            }
            var carrier = await _csfeApiClient.GetOrganizationsByCodeAsync(orgCode);
            return carrier != null;
        }
    }
}
