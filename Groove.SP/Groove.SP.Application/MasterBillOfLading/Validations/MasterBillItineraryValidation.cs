using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.MasterBillOfLading.ViewModels;

namespace Groove.SP.Application.MasterBillOfLading.Validations
{
    public class MasterBillItineraryValidation : BaseValidation<MasterBillOfLadingItineraryViewModel>
    {
        public MasterBillItineraryValidation(bool isUpdating = false)
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
            RuleFor(a => a.ItineraryId).NotNull();
            RuleFor(a => a.MasterBillOfLadingId).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.ItineraryId).NotEmpty();
            RuleFor(a => a.MasterBillOfLadingId).NotEmpty().When(x => x.IsPropertyDirty("MasterBillOfLadingId"));
        }
    }
}
