using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.ViewModels;

namespace Groove.SP.Application.Consignment.Validations
{
    public class ConsignmentItineraryValidation : BaseValidation<ConsignmentItineraryViewModel>
    {
        public ConsignmentItineraryValidation(bool isUpdating = false)
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
            RuleFor(a => a.ConsignmentId).NotNull();
            RuleFor(a => a.Sequence).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.ItineraryId).NotEmpty();
            RuleFor(a => a.ConsignmentId).NotEmpty().When(x => x.IsPropertyDirty("ConsignmentId"));
            RuleFor(a => a.Sequence).NotNull().When(x => x.IsPropertyDirty("Sequence"));
        }
    }
}
