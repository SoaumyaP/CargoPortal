using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.Activity.ViewModels
{
    public class AgentActivityCreateViewModel : ViewModelBase<ActivityModel>, IValidatableObject
    {
        public string ActivityCode { get; set; }

        public DateTime ActivityDate { get; set; }

        public string Location { get; set; }

        public string Remark { get; set; }

        public string CustomerCode { get; set; }

        public string ShipmentNo { get; set; }

        public string PurchaseOrderNo { get; set; }

        public string ContainerNo { get; set; }

        public bool? Resolved { get; set; }

        public string Resolution { get; set; }

        public DateTime? ResolutionDate { get; set; }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<AgentActivityCreateViewModel>)validationContext.GetService(typeof(IValidator<AgentActivityCreateViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }

        public override void ValidateAndThrow(bool isUpdating = false) { }
    }
}
