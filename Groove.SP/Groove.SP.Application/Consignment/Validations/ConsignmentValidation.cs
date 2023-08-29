using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.ViewModels;

namespace Groove.SP.Application.Consignment.Validations
{
    public class ConsignmentValidation : BaseValidation<ConsignmentViewModel>
    {
        public ConsignmentValidation(bool isUpdating = false)
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
            RuleFor(a => a.ShipmentId).NotEmpty();
            RuleFor(a => a.ConsignmentDate).NotEmpty();
            RuleFor(a => a.ExecutionAgentId).NotEmpty();
            RuleFor(a => a.ShipFromETDDate).NotEmpty();
            RuleFor(a => a.ShipToETADate).NotEmpty();
            RuleFor(a => a.Package).NotNull();
            RuleFor(a => a.Unit).NotNull();
            RuleFor(a => a.Volume).NotNull();
            RuleFor(a => a.GrossWeight).NotNull();
            RuleFor(a => a.NetWeight).NotNull();
            RuleFor(a => a.TriangleTradeFlag).NotNull();
            RuleFor(a => a.MemoBOLFlag).NotNull();
            RuleFor(a => a.Sequence).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.ShipmentId).NotEmpty().When(x => x.IsPropertyDirty("ShipmentId"));
            RuleFor(a => a.ConsignmentDate).NotEmpty().When(x => x.IsPropertyDirty("ConsignmentDate"));
            RuleFor(a => a.ExecutionAgentId).NotEmpty().When(x => x.IsPropertyDirty("ExecutionAgentId"));
            RuleFor(a => a.ShipFromETDDate).NotEmpty().When(x => x.IsPropertyDirty("ShipFromETDDate"));
            RuleFor(a => a.ShipToETADate).NotEmpty().When(x => x.IsPropertyDirty("ShipToETADate"));
            RuleFor(a => a.Package).NotNull().When(x => x.IsPropertyDirty("Package"));
            RuleFor(a => a.Unit).NotNull().When(x => x.IsPropertyDirty("Unit"));
            RuleFor(a => a.Volume).NotNull().When(x => x.IsPropertyDirty("Volume"));
            RuleFor(a => a.GrossWeight).NotNull().When(x => x.IsPropertyDirty("GrossWeight"));
            RuleFor(a => a.NetWeight).NotNull().When(x => x.IsPropertyDirty("NetWeight"));
            RuleFor(a => a.TriangleTradeFlag).NotNull().When(x => x.IsPropertyDirty("TriangleTradeFlag"));
            RuleFor(a => a.MemoBOLFlag).NotNull().When(x => x.IsPropertyDirty("MemoBOLFlag"));
            RuleFor(a => a.Sequence).NotNull().When(x => x.IsPropertyDirty("Sequence"));
        }
    }
}
