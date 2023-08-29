using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.RoutingOrderContact.ViewModels;
using Groove.SP.Core.Models;
using System.Collections.Generic;

namespace Groove.SP.Application.RoutingOrderContact.Validations
{
    public class ImportRoutingOrderContactViewModelValidator : BaseValidation<ImportRoutingOrderContactViewModel>
    {
        private readonly List<string> validOrgRoles = new()
        {
            OrganizationRole.Principal,
            OrganizationRole.Shipper,
            OrganizationRole.Consignee,
            OrganizationRole.Supplier,
            OrganizationRole.DestinationAgent
        };
        public ImportRoutingOrderContactViewModelValidator()
        {
            RuleFor(x => x.OrganizationRole).NotEmpty();
            RuleFor(x => x.OrganizationRole)
                .Must(organizationRole => validOrgRoles.Contains(organizationRole))
                .When(x => !string.IsNullOrWhiteSpace(x.OrganizationRole))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", validOrgRoles)}.");

            RuleFor(x => x.OrganizationCode).NotEmpty();

            RuleFor(x => x.CompanyName).MaximumLength(40);

            RuleFor(x => x.AddressLine1).MaximumLength(50);
            RuleFor(x => x.AddressLine2).MaximumLength(50);
            RuleFor(x => x.AddressLine3).MaximumLength(50);
            RuleFor(x => x.AddressLine4).MaximumLength(50);
            RuleFor(x => x.ContactName).MaximumLength(30);
            RuleFor(x => x.ContactNumber).MaximumLength(30);

            RuleFor(x => x.ContactEmail).MaximumLength(100)
                .Must(x => CommonValidators.CheckValidEmails(x))
                .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail) && !x.ContactEmail.Contains(';'))
                .WithMessage("Some of the emails provided are not valid");

            RuleFor(x => x.ContactEmail).MaximumLength(100)
                .Must(x => CommonValidators.CheckValidEmails(x, ';'))
                .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail) && x.ContactEmail.Contains(';'))
                .WithMessage("Some of the emails provided are not valid");
        }
    }
}