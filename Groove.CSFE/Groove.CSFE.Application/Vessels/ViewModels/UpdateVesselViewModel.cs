using FluentValidation;
using Groove.CSFE.Application.Converters.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Groove.CSFE.Application.Vessels.ViewModels
{
    public class UpdateVesselViewModel: VesselViewModel, IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<UpdateVesselViewModel>)validationContext.GetService(typeof(IValidator<UpdateVesselViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
