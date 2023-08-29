using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Core.Models;
using System.Linq;

namespace Groove.SP.Application.ShipmentContact.Validations;

public class ImportShipmentContactValidator : BaseValidation<ImportShipmentContactViewModel>
{
    private readonly string[] ValidRoles =
    {
        OrganizationRole.Principal,
        OrganizationRole.Shipper,
        OrganizationRole.Supplier,
        OrganizationRole.Consignee,
        OrganizationRole.OriginAgent,
        OrganizationRole.DestinationAgent
    };

    public ImportShipmentContactValidator()
    {
        RuleFor(x => x.OrganizationRole).NotEmpty();
        RuleFor(x => x.OrganizationCode).NotEmpty();
        RuleFor(x => x.OrganizationRole)
            .Must(x => ValidRoles.Contains(x))
            .WithMessage($"'{nameof(ImportShipmentContactViewModel.OrganizationRole)}' must be '{string.Join(", ", ValidRoles)}'.")
            .When(x => !string.IsNullOrEmpty(x.OrganizationRole));

        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .When(x => x.OrganizationCode == "0");
    }
}