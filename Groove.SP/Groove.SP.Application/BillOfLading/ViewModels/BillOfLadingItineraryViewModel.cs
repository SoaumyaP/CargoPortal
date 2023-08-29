using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Application.BillOfLading.Validations;
using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.BillOfLading.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class BillOfLadingItineraryViewModel : ViewModelBase<BillOfLadingItineraryModel>, IHasFieldStatus
    {
        public long ItineraryId { get; set; }

        public long BillOfLadingId { get; set; }

        public long? MasterBillOfLadingId { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new BillOfLadingItineraryValidation(isUpdating).ValidateAndThrow(this);
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
