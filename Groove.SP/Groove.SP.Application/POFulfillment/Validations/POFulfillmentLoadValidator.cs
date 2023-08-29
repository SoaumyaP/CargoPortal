using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class POFulfillmentLoadValidator : BaseValidation<POFulfillmentLoadViewModel>
    {
        public POFulfillmentLoadValidator()
        {
            RuleFor(a => a.EquipmentType).NotNull();
            RuleFor(a => a.PlannedVolume).NotEmpty();
            RuleFor(a => a.PlannedPackageQuantity).NotEmpty();
            RuleFor(a => a.PackageUOM).NotNull();
        }
    }
}
