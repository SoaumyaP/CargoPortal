using FluentValidation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.Itinerary.ViewModels
{
    public class CreateItineraryViewModel : ItineraryViewModel, IValidatableObject
    {

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<CreateItineraryViewModel>)validationContext.GetService(typeof(IValidator<CreateItineraryViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }
    }
}
