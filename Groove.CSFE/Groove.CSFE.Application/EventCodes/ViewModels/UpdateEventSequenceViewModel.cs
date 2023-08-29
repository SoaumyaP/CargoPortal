using FluentValidation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Groove.CSFE.Application.EventCodes.ViewModels
{
    public class UpdateEventSequenceViewModel : IValidatableObject
    {
        public string ActivityCode { get; set; }
        public long SortSequence { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validator = (IValidator<UpdateEventSequenceViewModel>)
                validationContext.GetService(typeof(IValidator<UpdateEventSequenceViewModel>));
            var result = validator.Validate(this);

            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }
    }
}