using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Countries.ViewModels;

namespace Groove.CSFE.Application.Countries.Validations
{
    public class CountryValidation : BaseValidation<CountryViewModel>
    {
        public CountryValidation(bool isUpdating = false)
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
            RuleFor(a => a.Code).NotEmpty();
            RuleFor(a => a.Name).NotEmpty();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty().When(x => x.IsPropertyDirty("Id"));
            RuleFor(a => a.Code).NotEmpty().When(x => x.IsPropertyDirty("Code"));
            RuleFor(a => a.Name).NotEmpty().When(x => x.IsPropertyDirty("Name"));
        }
    }
}
