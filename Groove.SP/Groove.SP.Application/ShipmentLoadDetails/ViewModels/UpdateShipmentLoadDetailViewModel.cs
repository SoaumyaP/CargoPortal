using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.ShipmentLoadDetails.ViewModels
{
    public class UpdateShipmentLoadDetailViewModel : ViewModelBase<ShipmentLoadDetailModel>, IValidatableObject
    {
        public long Id { get; set; }

        public decimal Package { get; set; }

        public decimal Unit { get; set; }

        public decimal Volume { get; set; }

        public decimal GrossWeight { get; set; }

        public int Sequence { get; set; }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<UpdateShipmentLoadDetailViewModel>)validationContext.GetService(typeof(IValidator<UpdateShipmentLoadDetailViewModel>));
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
