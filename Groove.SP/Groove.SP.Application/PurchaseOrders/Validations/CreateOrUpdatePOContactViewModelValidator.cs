using System;
using System.Linq;

using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using FluentValidation;
using Groove.SP.Infrastructure.CSFE;
using System.Collections.Generic;
using Groove.SP.Infrastructure.CSFE.Models;
using OrganizationRoleConstant = Groove.SP.Core.Models.OrganizationRole;

namespace Groove.SP.Application.PurchaseOrders.Validations
{
    public class CreateOrUpdatePOContactViewModelValidator : BaseValidation<CreateOrUpdatePOContactViewModel>
    {
        public CreateOrUpdatePOContactViewModelValidator(ICSFEApiClient csfeApiClient)
        {
            var roles = csfeApiClient.GetAllOrganizationRolesAsync().Result;
            var organizations = csfeApiClient.GetActiveOrganizationsAsync().Result;

            RuleFor(a => a.OrganizationRole).NotEmpty();
            RuleFor(a => a.OrganizationCode).NotEmpty();

            RuleFor(a => a.OrganizationRole)
                .Must(x => roles.Any(r => r.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Inputted data is not existing in system.");

            RuleFor(a => a.OrganizationCode)
               .Must((viewModel, property) => ValidateOrgCode(viewModel, organizations))
               .WithMessage($"Inputted data is not existing in system.");
        }

        private bool ValidateOrgCode(CreateOrUpdatePOContactViewModel viewModel, IEnumerable<Organization> organizations)
        {
            var isValidOrgCode = organizations.Any(r => r.Code.Equals(viewModel.OrganizationCode, StringComparison.OrdinalIgnoreCase));
            if (viewModel.OrganizationRole.Equals(OrganizationRoleConstant.Principal, StringComparison.OrdinalIgnoreCase))
            {
                return isValidOrgCode;
            }
            else
            {
                return viewModel.OrganizationCode == "0" ? true : isValidOrgCode;
            }
        }
    }
}
