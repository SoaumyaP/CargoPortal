using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;

namespace Groove.SP.Application.BuyerCompliance.Validations
{
    public class ActivateBuyerComplianceValidator : BaseValidation<ActivateBuyerComplianceViewModel>
    {
        public ActivateBuyerComplianceValidator()
        {
            RuleFor(a => a.OrganizationId).NotEmpty();
            RuleFor(a => a.Name).NotEmpty();
            RuleFor(a => a.ShortShipTolerancePercentage).NotEmpty();
            RuleFor(a => a.OvershipTolerancePercentage).NotEmpty();
            RuleFor(a => a.ApprovalAlertFrequency).NotEmpty();
            RuleFor(a => a.ApprovalDuration).NotNull();
            RuleFor(a => a.BookingTimeless).SetValidator(new BookingTimelessValidator());
            RuleFor(a => a.CargoLoadabilities).NotNull().Must(p => p.Count > 0);
            RuleForEach(a => a.CargoLoadabilities).SetValidator(new CargoLoadabilityValidator());
        }
    }
}
