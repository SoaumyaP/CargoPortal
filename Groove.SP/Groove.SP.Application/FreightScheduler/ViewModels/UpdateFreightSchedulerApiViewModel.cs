using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Groove.SP.Application.FreightScheduler.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class UpdateFreightSchedulerApiViewModel : ViewModelBase<FreightSchedulerModel>, IValidatableObject, IHasFieldStatus
    {
        public string ModeOfTransport { set; get; }
        public string CarrierCode { set; get; }
        public string VesselName { set; get; }
        public string Voyage { set; get; }
        public string LocationToCode { get; set; }
        public DateTime ETADate { get; set; }
        public DateTime? ATADate { get; set; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { set; get; }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<UpdateFreightSchedulerApiViewModel>)validationContext.GetService(typeof(IValidator<UpdateFreightSchedulerApiViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            
        }
    }
}