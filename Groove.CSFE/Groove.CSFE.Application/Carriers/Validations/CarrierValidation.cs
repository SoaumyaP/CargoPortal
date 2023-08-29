using FluentValidation;
using Groove.CSFE.Application.Carriers.ViewModels;
using Groove.CSFE.Application.Common;

namespace Groove.CSFE.Application.Carriers.Validations
{
    public class CarrierValidation : BaseValidation<CarrierViewModel>
    {
        public CarrierValidation(bool isUpdating = false)
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
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotNull();
        }
    }
}
