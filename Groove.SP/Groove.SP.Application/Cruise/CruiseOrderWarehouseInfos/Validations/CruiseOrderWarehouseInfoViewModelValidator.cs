using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.ViewModels;

namespace Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Validations
{
    public class CruiseOrderWarehouseInfoViewModelValidator : BaseValidation<CruiseOrderWarehouseInfoViewModel>
    {
        public CruiseOrderWarehouseInfoViewModelValidator(bool isUpdating = false)
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
            RuleFor(a => a.CruiseOrderId).NotNull();
            RuleFor(a => a.POLine).NotNull();
            RuleFor(a => a.RefID).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotNull();
            RuleFor(a => a.CruiseOrderId).NotNull().When(x => x.IsPropertyDirty(nameof(CruiseOrderWarehouseInfoViewModel.CruiseOrderId)));
            RuleFor(a => a.POLine).NotNull().When(x => x.IsPropertyDirty(nameof(CruiseOrderWarehouseInfoViewModel.POLine)));
            RuleFor(a => a.RefID).NotNull().When(x => x.IsPropertyDirty(nameof(CruiseOrderWarehouseInfoViewModel.RefID)));
        }
    }
}
