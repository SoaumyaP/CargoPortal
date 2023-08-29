using FluentValidation;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class POFulfillmentOrderValidator : AbstractValidator<POFulfillmentOrderViewModel>
    {
        public POFulfillmentOrderValidator(OrderFulfillmentPolicy fulfillmentPolicy)
        {
            // Allow to add missing PO without validation, assume that missing PO has PurchaseOrderId = 0
            RuleFor(a => a.FulfillmentUnitQty).GreaterThan(0).When(x => x.PurchaseOrderId != 0 || fulfillmentPolicy == OrderFulfillmentPolicy.NotAllowMissingPO);
            RuleFor(a => a.BookedPackage).NotNull().When(x => x.PurchaseOrderId != 0 || fulfillmentPolicy == OrderFulfillmentPolicy.NotAllowMissingPO);
            RuleFor(a => a.BookedPackage).GreaterThanOrEqualTo(0); // Not allow negative integer
        }
    }
}