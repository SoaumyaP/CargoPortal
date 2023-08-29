using FluentValidation;
using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.Common;

namespace Groove.SP.Application.BuyerApproval.Validations
{
    public class BuyerApprovalValidation : BaseValidation<BuyerApprovalViewModel>
    {
        public BuyerApprovalValidation(bool isUpdating = false)
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
        }
    }
}
