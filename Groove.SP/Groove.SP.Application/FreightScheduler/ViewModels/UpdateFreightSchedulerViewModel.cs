using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.FreightScheduler.Validations;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Groove.SP.Application.FreightScheduler.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class UpdateFreightSchedulerViewModel : ViewModelBase<FreightSchedulerModel>, IHasFieldStatus
    {
        public long Id { set; get; }
        
        public DateTime ETDDate { set; get; }
        public DateTime ETADate { set; get; }
        public DateTime? ATDDate { set; get; }
        public DateTime? ATADate { set; get; }
        public DateTime? CYOpenDate { set; get; }
        public DateTime? CYClosingDate { set; get; }
        public string LocationFromName { set; get; }
        public string LocationToName { set; get; }
        public bool IsAllowExternalUpdate { get; set; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new UpdateFreightSchedulerValidation(isUpdating).ValidateAndThrow(this);
        }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
