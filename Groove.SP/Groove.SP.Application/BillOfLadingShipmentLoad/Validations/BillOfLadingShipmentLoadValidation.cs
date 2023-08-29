using FluentValidation;
using Groove.SP.Application.BillOfLadingShipmentLoad.ViewModels;
using Groove.SP.Application.Common;

namespace Groove.SP.Application.BillOfLadingShipmentLoad.Validations
{
    public class BillOfLadingShipmentLoadValidation : BaseValidation<BillOfLadingShipmentLoadViewModel>
    {
        public BillOfLadingShipmentLoadValidation(bool isUpdating = false)
        {
            if (isUpdating)
            {
                ValidateUpdate();
            }
            else
            {
                ValidateAdd();
            }
        }

        private void ValidateAdd()
        {
            RuleFor(x => x.BillOfLadingId).NotNull();
            RuleFor(x => x.ShipmentLoadId).NotNull();
            RuleFor(x => x.IsFCL).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(x => x.BillOfLadingId).NotEmpty();
            RuleFor(x => x.ShipmentLoadId).NotEmpty().When(x => x.IsPropertyDirty("ShipmentLoadId"));
            RuleFor(x => x.IsFCL).NotNull().When(x => x.IsPropertyDirty("IsFCL"));
        }
    }
}
