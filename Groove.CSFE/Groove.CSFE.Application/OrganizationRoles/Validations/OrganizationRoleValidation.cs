using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.OrganizationRoles.ViewModels;

namespace Groove.CSFE.Application.OrganizationRoles.Validations
{
    public class OrganizationRoleValidation : BaseValidation<OrganizationRoleViewModel>
    {
        public OrganizationRoleValidation(bool isUpdating = false)
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
            RuleFor(a => a.OrganizationTypes).NotEmpty();
            RuleFor(x => x.CreatedDate).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.Name).NotEmpty().When(x => x.IsPropertyDirty("Name"));
            RuleFor(a => a.OrganizationTypes).NotEmpty().When(x => x.IsPropertyDirty("OrganizationTypes"));
        }
    }
}
