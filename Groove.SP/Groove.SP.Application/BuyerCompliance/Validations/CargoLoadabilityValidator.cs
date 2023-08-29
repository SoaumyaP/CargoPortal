using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;

namespace Groove.SP.Application.BuyerCompliance.Validations
{
    public class CargoLoadabilityValidator : BaseValidation<CargoLoadabilityViewModel>
    {
        public CargoLoadabilityValidator()
        {
            RuleFor(a => a.CyMinimumCBM).NotNull().LessThan(a => a.CyMaximumCBM);
            RuleFor(a => a.CyMaximumCBM).NotNull();
            RuleFor(a => a.CfsMinimumCBM).NotNull().When(a => a.CfsMinimumCBM != null);
            RuleFor(a => a.CfsMaximumCBM).NotNull().When(a => a.CfsMaximumCBM != null);
        }
    }
}
