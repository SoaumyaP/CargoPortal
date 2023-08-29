using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Application.Consignment.Validations;
using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.Consignment.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ConsignmentItineraryViewModel : ViewModelBase<ConsignmentItineraryModel>, IHasFieldStatus
    {      
        public long ConsignmentId { get; set; }

        public long ItineraryId { get; set; }

        public long? ShipmentId { get; set; }

        public long? MasterBillId { get; set; }

        public int? Sequence { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ConsignmentItineraryValidation(isUpdating).ValidateAndThrow(this);
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
