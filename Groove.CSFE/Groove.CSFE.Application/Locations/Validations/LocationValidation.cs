using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Locations.ViewModels;
using FluentValidation;

namespace Groove.CSFE.Application.Locations.Validations
{
    public class LocationValidation : BaseValidation<LocationViewModel>
    {
        public LocationValidation(bool isUpdating = false)
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
            RuleFor(a => a.CountryId).NotEmpty();
            RuleFor(a => a.Name).NotEmpty();
            RuleFor(a => a.LocationDescription).NotEmpty();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty().When(x => x.IsPropertyDirty("Id"));
            RuleFor(a => a.CountryId).NotEmpty().When(x => x.IsPropertyDirty("CountryId"));
            RuleFor(a => a.Name).NotEmpty().When(x => x.IsPropertyDirty("Name"));
            RuleFor(a => a.LocationDescription).NotEmpty().When(x => x.IsPropertyDirty("LocationDescription"));
        }
    }
}
