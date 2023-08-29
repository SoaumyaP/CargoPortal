using FluentValidation;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.Common;

namespace Groove.SP.Application.BillOfLading.Validations
{
    public class BillOfLadingValidation : BaseValidation<BillOfLadingViewModel>
    {
        public BillOfLadingValidation(bool isUpdating = false)
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
            RuleFor(a => a.BillOfLadingNo).NotEmpty();
            RuleFor(a => a.ExecutionAgentId).NotNull();
            RuleFor(a => a.TotalGrossWeight).NotNull();
            RuleFor(a => a.TotalNetWeight).NotNull();
            RuleFor(a => a.TotalPackage).NotNull();
            RuleFor(a => a.TotalVolume).NotNull();
            RuleFor(a => a.IssueDate).NotNull();
            RuleFor(a => a.ShipFromETDDate).NotNull();
            RuleFor(a => a.ShipToETADate).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.BillOfLadingNo).NotEmpty().When(x => x.IsPropertyDirty("BillOfLadingNo"));
            RuleFor(a => a.TotalGrossWeight).NotNull().When(x => x.IsPropertyDirty("TotalGrossWeight"));
            RuleFor(a => a.TotalNetWeight).NotNull().When(x => x.IsPropertyDirty("TotalNetWeight"));
            RuleFor(a => a.TotalPackage).NotNull().When(x => x.IsPropertyDirty("TotalPackage"));
            RuleFor(a => a.TotalVolume).NotNull().When(x => x.IsPropertyDirty("TotalVolume"));
            RuleFor(a => a.IssueDate).NotNull().When(x => x.IsPropertyDirty("IssueDate"));
            RuleFor(a => a.ShipFromETDDate).NotNull().When(x => x.IsPropertyDirty("ShipFromETDDate"));
            RuleFor(a => a.ShipToETADate).NotNull().When(x => x.IsPropertyDirty("ShipToETADate"));
        }
    }
}
