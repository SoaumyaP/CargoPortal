using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentBillOfLading.ViewModels;

namespace Groove.SP.Application.ShipmentBillOfLading.Validations
{
    public class ShipmentBillOfLadingValidation : BaseValidation<ShipmentBillOfLadingViewModel>
    {
        public ShipmentBillOfLadingValidation(bool isUpdating = false)
        {
        }
    }
}
