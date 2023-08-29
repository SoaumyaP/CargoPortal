using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Invoices.ViewModels;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Invoices.Validations
{
    public class InvoiceUpdatePaymentViewModelValidator : BaseValidation<InvoiceUpdatePaymentViewModel>
    {
        public List<PaymentStatusType?> ValidPaymentStatus { get; } = new List<PaymentStatusType?>() { PaymentStatusType.Paid, PaymentStatusType.Partial };

        public InvoiceUpdatePaymentViewModelValidator()
        {
            RuleFor(a => a.InvoiceNo).NotEmpty();
            RuleFor(a => a.InvoiceNo).MaximumLength(35);
            RuleFor(a => a.PaymentStatus)
                    .Must((v, p) => v.IsPropertyDirty(nameof(InvoiceUpdatePaymentViewModel.PaymentStatus)))
                    .WithMessage("PaymentStatus is required");
            RuleFor(a => a.PaymentStatus)
                .Must(c => ValidPaymentStatus.Contains(c))
                .When(c => c.PaymentStatus.HasValue)
                .WithMessage("Invalid PaymentStatus");

            RuleFor(a => a.PaymentDate).NotEmpty().When(c => ValidPaymentStatus.Contains(c.PaymentStatus));
            RuleFor(a => a.UpdatedBy).MaximumLength(256);
        }
    }
}
