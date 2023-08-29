using Groove.SP.Application.Activity.Validations;
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

namespace Groove.SP.Application.Activity.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ActivityViewModel : ViewModelBase<ActivityModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public string ActivityCode { get; set; }

        public long? ShipmentId { get; set; }

        public long? ContainerId { get; set; }

        public long? ConsignmentId { get; set; }

        public long? PurchaseOrderId { get; set; }

        public long? POFulfillmentId { get; set; }

        public long? CruiseOrderId { get; set; }

        public long? FreightSchedulerId { get; set; }

        public string ActivityType { get; set; }

        public string ActivityDescription { get; set; }

        public DateTime ActivityDate { get; set; }

        public string ActivityLevel { get; set; }

        public string Location { get; set; }

        public string Remark { get; set; }

        public bool? Resolved { get; set; }

        public string Resolution { get; set; }

        public DateTime? ResolutionDate { get; set; }

        public long SortSequence { get; set; }

        /// <summary>
        /// Some additional information to describe current activity. It can be used to proceed specific business
        /// </summary>
        public string MetaData { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ActivityValidation(isUpdating).ValidateAndThrow(this);
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
