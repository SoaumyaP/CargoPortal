using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.Validations;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class ImportPOFulfillmentCargoReceiveViewModel : ViewModelBase<POFulfillmentCargoReceiveModel>, IValidatableObject
    {
        public string BookingNo { get; set; }
        public string CRNo { get; set; }
        public string PlantNo { get; set; }
        public string HouseNo { get; set; }
        public IEnumerable<ImportPOFulfillmentCargoReceiveItemViewModel> CustomerPO { get; set; }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<ImportPOFulfillmentCargoReceiveViewModel>)validationContext.GetService(typeof(IValidator<ImportPOFulfillmentCargoReceiveViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ImportPOFulfillmentCargoReceiveViewModelValidator().ValidateAndThrow(this);
        }
    }
}