using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using FluentValidation;
using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.Activity.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class AgentActivityUpdateViewModel : ViewModelBase<ActivityModel>, IValidatableObject, IHasFieldStatus
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
            var validator = (IValidator<AgentActivityUpdateViewModel>)validationContext.GetService(typeof(IValidator<AgentActivityUpdateViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }

        public override void ValidateAndThrow(bool isUpdating = false) { }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
