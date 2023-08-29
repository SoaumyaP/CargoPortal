using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.MasterBillOfLadingContact.ViewModels;

namespace Groove.SP.Application.MasterBillOfLadingContact.Validations
{
    public class MasterBillContactValidation : BaseValidation<MasterBillOfLadingContactViewModel>
    {
        public MasterBillContactValidation(bool isUpdating = false)
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
            RuleFor(a => a.MasterBillOfLadingId).NotNull();
            RuleFor(a => a.OrganizationId).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.MasterBillOfLadingId).NotEmpty().When(x => x.IsPropertyDirty("MasterBillOfLadingId"));
            RuleFor(a => a.OrganizationId).NotNull().When(x => x.IsPropertyDirty("OrganizationId"));
        }
    }
}
