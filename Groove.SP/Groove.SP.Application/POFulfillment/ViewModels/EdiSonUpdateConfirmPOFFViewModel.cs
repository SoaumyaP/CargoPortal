using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class EdiSonUpdateConfirmPOFFViewModel : IHasFieldStatus, IValidatableObject
    {
        public string BookingReferenceNo { get; set; }

        public string CYEmptyPickupTerminalCode { get; set; }

        public string CYEmptyPickupTerminalDescription { get; set; }

        public string CFSWarehouseCode { get; set; }

        public string CFSWarehouseDescription { get; set; }

        public DateTime? CYClosingDate { get; set; }

        public DateTime? CFSClosingDate { get; set; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validator = (IValidator<EdiSonUpdateConfirmPOFFViewModel>)validationContext.GetService(typeof(IValidator<EdiSonUpdateConfirmPOFFViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }
    }
}