using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.BillOfLading.ViewModels;

namespace Groove.SP.Application.BillOfLading.Validations
{
    public class BillOfLadingItineraryValidation : BaseValidation<BillOfLadingItineraryViewModel>
    {
        public BillOfLadingItineraryValidation(bool isUpdating = false)
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
            RuleFor(a => a.BillOfLadingId).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.ItineraryId).NotEmpty();
            RuleFor(a => a.BillOfLadingId).NotEmpty().When(x => x.IsPropertyDirty("BillOfLadingId"));
        }
    }
}
