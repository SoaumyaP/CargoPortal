using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;

namespace Groove.SP.Application.BuyerCompliance.Validations
{
    public class SaveBuyerComplianceValidator : BaseValidation<SaveBuyerComplianceViewModel>
    {
        public SaveBuyerComplianceValidator()
        {
            RuleFor(a => a.OrganizationId).NotNull();
            RuleFor(a => a.Name).NotEmpty();
        }
    }
}
