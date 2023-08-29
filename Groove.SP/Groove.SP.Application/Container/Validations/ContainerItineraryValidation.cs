using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Container.ViewModels;

namespace Groove.SP.Application.Container.Validations
{
    public class ContainerItineraryValidation : BaseValidation<ContainerItineraryViewModel>
    {
        public ContainerItineraryValidation(bool isUpdating = false)
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
            RuleFor(a => a.ContainerId).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.ItineraryId).NotEmpty();
            RuleFor(a => a.ContainerId).NotEmpty().When(x => x.IsPropertyDirty("ContainerId"));
        }
    }
}