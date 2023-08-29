using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;

namespace Groove.SP.Application.ShipmentLoadDetails.Validations
{
    public class ShipmentLoadDetailValidation : BaseValidation<ShipmentLoadDetailViewModel>
    {
        public ShipmentLoadDetailValidation(bool isUpdating = false)
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
            RuleFor(a => a.GrossWeight).NotNull();
            RuleFor(a => a.NetWeight).NotNull();
            RuleFor(a => a.Package).NotNull();
            RuleFor(a => a.Volume).NotNull();
            RuleFor(a => a.Unit).NotNull();
            RuleFor(a => a.Sequence).NotNull();
            RuleFor(a => a.Sequence).GreaterThan(0);
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.GrossWeight).NotNull().When(x => x.IsPropertyDirty("GrossWeight"));
            RuleFor(a => a.NetWeight).NotNull().When(x => x.IsPropertyDirty("NetWeight"));
            RuleFor(a => a.Package).NotNull().When(x => x.IsPropertyDirty("Package"));
            RuleFor(a => a.Volume).NotNull().When(x => x.IsPropertyDirty("Volume"));
            RuleFor(a => a.Unit).NotNull().When(x => x.IsPropertyDirty("Unit"));
            RuleFor(a => a.Sequence).NotNull().When(x => x.IsPropertyDirty("Sequence"));
            RuleFor(a => a.Sequence).GreaterThan(0).When(x => x.IsPropertyDirty("Sequence"));
        }
    }
}
