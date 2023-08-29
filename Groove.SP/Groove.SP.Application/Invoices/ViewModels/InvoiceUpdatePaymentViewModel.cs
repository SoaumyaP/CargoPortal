using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Invoices.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class InvoiceUpdatePaymentViewModel: IHasFieldStatus, IValidatableObject
    {
        public string InvoiceNo { get; set; }
        public PaymentStatusType? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<InvoiceUpdatePaymentViewModel>)validationContext.GetService(typeof(IValidator<InvoiceUpdatePaymentViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }
    }
}
