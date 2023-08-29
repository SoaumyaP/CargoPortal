using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.Activity.ViewModels
{
    public class AgentActivityDeleteViewModel : ViewModelBase<ActivityModel>, IValidatableObject
    {
        public string ActivityCode { get; set; }

        public string CustomerCode { get; set; }

        public string ShipmentNo { get; set; }

        public string PurchaseOrderNo { get; set; }

        public string ContainerNo { get; set; }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<AgentActivityDeleteViewModel>)validationContext.GetService(typeof(IValidator<AgentActivityDeleteViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }

        public override void ValidateAndThrow(bool isUpdating = false) { }
    }
}
