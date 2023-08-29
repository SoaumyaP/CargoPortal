using FluentValidation;
using Groove.SP.Application.POFulfillment.ViewModels;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class ImportBookingLoadValidator : AbstractValidator<POFulfillmentLoadViewModel>
    {
        public ImportBookingLoadValidator()
        {
            RuleFor(a => a.EquipmentType).NotEmpty();
            RuleFor(a => a.PlannedVolume).NotEmpty();
            RuleFor(a => a.PlannedGrossWeight).NotEmpty();
            RuleFor(a => a.PlannedPackageQuantity).NotEmpty();
            RuleFor(a => a.PackageUOM).NotEmpty();
        }
    }
}
