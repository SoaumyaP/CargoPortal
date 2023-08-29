using FluentValidation;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Groove.SP.Application.MasterBillOfLading.ViewModels
{
    public class CreateMasterBillOfLadingViewModel : MasterBillOfLadingViewModel, IHasFieldStatus, IValidatableObject
    {
        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<CreateMasterBillOfLadingViewModel>)validationContext.GetService(typeof(IValidator<CreateMasterBillOfLadingViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }
    }
}
