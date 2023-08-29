using FluentValidation;
using Groove.SP.Application.BillOfLadingContact.ViewModels;
using Groove.SP.Application.Common;

namespace Groove.SP.Application.BillOfLadingContact.Validations
{
    public class BillOfLadingContactValidation : BaseValidation<BillOfLadingContactViewModel>
    {
        public BillOfLadingContactValidation(bool isUpdating = false)
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
            RuleFor(x => x.Id).NotNull();
            RuleFor(x => x.BillOfLadingId).NotNull();
            RuleFor(x => x.OrganizationId).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(x => x.Id).NotNull();
            RuleFor(x => x.BillOfLadingId).NotNull();
            RuleFor(x => x.OrganizationId).NotNull();
        }
    }
}
