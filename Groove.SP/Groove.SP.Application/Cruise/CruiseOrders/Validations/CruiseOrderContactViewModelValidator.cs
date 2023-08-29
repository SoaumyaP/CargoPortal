using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.ViewModels;

namespace Groove.SP.Application.CruiseOrders.Validations
{
    public class CruiseOrderContactViewModelValidator : BaseValidation<CruiseOrderContactViewModel>
    {
        public CruiseOrderContactViewModelValidator()
        {
            RuleFor(x => x.OrganizationRole).NotEmpty();
            RuleFor(x => x.CompanyName).NotEmpty();
            RuleFor(x => x.OrganizationId).NotNull();
        }
    }
}
