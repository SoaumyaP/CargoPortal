using FluentValidation;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;

namespace Groove.SP.Application.Activity.Validations
{
    public class AgentActivityDeleteViewModelValidator : BaseValidation<AgentActivityDeleteViewModel>
    {
        public AgentActivityDeleteViewModelValidator()
        {
            RuleFor(a => a.ActivityCode)
                .NotEmpty();

            RuleFor(a => a.CustomerCode)
                .NotEmpty()
                .When(a => !string.IsNullOrEmpty(a.ShipmentNo) || !string.IsNullOrEmpty(a.PurchaseOrderNo));

            RuleFor(a => a)
                .Must(a => (!string.IsNullOrEmpty(a.ShipmentNo) && string.IsNullOrEmpty(a.PurchaseOrderNo) && string.IsNullOrEmpty(a.ContainerNo)) ||
                     (!string.IsNullOrEmpty(a.PurchaseOrderNo) && string.IsNullOrEmpty(a.ShipmentNo) && string.IsNullOrEmpty(a.ContainerNo)) ||
                     (!string.IsNullOrEmpty(a.ContainerNo) && string.IsNullOrEmpty(a.ShipmentNo) && string.IsNullOrEmpty(a.PurchaseOrderNo)))
                .WithMessage("Either ShipmentNo, PurchaseOrderNo, or ContainerNo is allowed.")
                .WithName("ShipmentNo | PurchaseOrderNo | ContainerNo")
                .When(a => !string.IsNullOrEmpty(a.ShipmentNo) || !string.IsNullOrEmpty(a.PurchaseOrderNo) || !string.IsNullOrEmpty(a.ContainerNo));

            RuleFor(a => a)
                .Must(a => !string.IsNullOrEmpty(a.ShipmentNo) || !string.IsNullOrEmpty(a.PurchaseOrderNo) || !string.IsNullOrEmpty(a.ContainerNo))
                .WithMessage("Either ShipmentNo, PurchaseOrderNo, or ContainerNo is required.")
                .WithName("ShipmentNo | PurchaseOrderNo | ContainerNo");
        }
    }
}
