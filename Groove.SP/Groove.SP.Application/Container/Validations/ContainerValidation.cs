using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Container.ViewModels;

namespace Groove.SP.Application.Container.Validations
{
    public class ContainerValidation : BaseValidation<ContainerViewModel>
    {
        public ContainerValidation(bool isUpdating = false)
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
            RuleFor(a => a.ContainerNo).NotEmpty();
            RuleFor(a => a.LoadPlanRefNo).NotEmpty();
            RuleFor(a => a.ShipFrom).NotEmpty();
            RuleFor(a => a.ShipTo).NotEmpty();
            RuleFor(a => a.ShipFromETDDate).NotNull();
            RuleFor(a => a.ShipFromETDDate).NotNull();
            RuleFor(a => a.TotalGrossWeight).NotNull();
            RuleFor(a => a.TotalNetWeight).NotNull();
            RuleFor(a => a.TotalPackage).NotNull();
            RuleFor(a => a.TotalVolume).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.ContainerNo).NotEmpty().When(x => x.IsPropertyDirty("ContainerNo"));
            RuleFor(a => a.LoadPlanRefNo).NotEmpty().When(x => x.IsPropertyDirty("LoadPlanRefNo"));
            RuleFor(a => a.ShipFrom).NotEmpty().When(x => x.IsPropertyDirty("ShipFrom"));
            RuleFor(a => a.ShipTo).NotEmpty().When(x => x.IsPropertyDirty("ShipTo"));
            RuleFor(a => a.ShipFromETDDate).NotNull().When(x => x.IsPropertyDirty("ShipFromETDDate"));
            RuleFor(a => a.ShipFromETDDate).NotNull().When(x => x.IsPropertyDirty("ShipFromETDDate"));
            RuleFor(a => a.TotalGrossWeight).NotNull().When(x => x.IsPropertyDirty("TotalGrossWeight"));
            RuleFor(a => a.TotalNetWeight).NotNull().When(x => x.IsPropertyDirty("TotalNetWeight"));
            RuleFor(a => a.TotalPackage).NotNull().When(x => x.IsPropertyDirty("TotalPackage"));
            RuleFor(a => a.TotalVolume).NotNull().When(x => x.IsPropertyDirty("TotalVolume"));
        }
    }
}
