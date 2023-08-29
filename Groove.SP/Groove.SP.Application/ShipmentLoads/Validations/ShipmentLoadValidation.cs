using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;

namespace Groove.SP.Application.ShipmentLoads.Validations
{
    public class ShipmentLoadValidation : BaseValidation<ShipmentLoadViewModel>
    {
        public ShipmentLoadValidation(bool isUpdating = false)
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
            RuleFor(a => a.Id).NotNull();
            RuleFor(a => a.IsFCL).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.IsFCL).NotNull().When(x => x.IsPropertyDirty("IsFCL"));
        }
    }
}
