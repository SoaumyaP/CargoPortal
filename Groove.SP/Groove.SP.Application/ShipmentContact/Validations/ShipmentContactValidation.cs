using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentContact.ViewModels;

namespace Groove.SP.Application.ShipmentContact.Validations
{
    public class ShipmentContactValidation : BaseValidation<ShipmentContactViewModel>
    {
        public ShipmentContactValidation(bool isUpdating = false)
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
            RuleFor(a => a.ShipmentId).NotNull();
            RuleFor(a => a.OrganizationId).NotNull();
            RuleFor(a => a.OrganizationRole).NotEmpty();
            RuleFor(a => a.CompanyName).NotEmpty();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.ShipmentId).NotEmpty().When(x => x.IsPropertyDirty("ShipmentId"));
            RuleFor(a => a.OrganizationId).NotNull().When(x => x.IsPropertyDirty("OrganizationId"));
            RuleFor(a => a.OrganizationRole).NotEmpty().When(x => x.IsPropertyDirty("OrganizationRole"));
            RuleFor(a => a.CompanyName).NotEmpty().When(x => x.IsPropertyDirty("CompanyName"));
        }
    }
}
