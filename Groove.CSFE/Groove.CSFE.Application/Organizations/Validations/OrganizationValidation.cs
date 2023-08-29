using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Organizations.ViewModels;

namespace Groove.CSFE.Application.Organizations.Validations
{
    public class OrganizationValidation : BaseValidation<OrganizationViewModel>
    {
        public OrganizationValidation(bool isUpdating = false)
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
            RuleFor(a => a.Name).NotEmpty();
            RuleFor(a => a.OrganizationType).NotNull();
            RuleFor(a => a.CustomerPrefix).Matches(@"^[A-Z0-9]{5}$").When(a => !string.IsNullOrEmpty(a.CustomerPrefix));
        }

        private void ValidateUpdate()
        {
            RuleFor(o => o.Id).NotEmpty().When(x => x.IsPropertyDirty("Id"));
            RuleFor(o => o.Code).NotEmpty().When(x => x.IsPropertyDirty("Code"));
            RuleFor(o => o.Name).NotEmpty().When(x => x.IsPropertyDirty("Name"));
            //RuleFor(o => o.Status).NotEmpty().When(x => x.IsPropertyDirty("Status"));
            RuleFor(o => o.OrganizationType).NotEmpty().When(x => x.IsPropertyDirty("OrganizationType"));
        }
    }
}
