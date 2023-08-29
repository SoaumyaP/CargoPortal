using Groove.SP.Application.Common;
using Groove.SP.Application.RoutingOrderInvoice.ViewModels;
using FluentValidation;

namespace Groove.SP.Application.RoutingOrderInvoice.Validations
{
    public class ImportRoutingOrderInvoiceViewModelValidator : BaseValidation<ImportRoutingOrderInvoiceViewModel>
    {
        public ImportRoutingOrderInvoiceViewModelValidator()
        {
            RuleFor(x => x.InvoiceType).MaximumLength(50);
            RuleFor(x => x.InvoiceNumber).MaximumLength(35);
        }
    }
}