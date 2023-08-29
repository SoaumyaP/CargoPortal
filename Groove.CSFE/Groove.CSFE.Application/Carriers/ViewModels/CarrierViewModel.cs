using FluentValidation;
using Groove.CSFE.Application.Carriers.Validations;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Converters.Interfaces;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using Newtonsoft.Json;
using Groove.CSFE.Application.Converters;
using System.Linq;

namespace Groove.CSFE.Application.Carriers.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class CarrierViewModel : ViewModelBase<CarrierModel>, IHasFieldStatus
    {
        public long Id { get; set; }
        public string CarrierCode { get; set; }
        public string ModeOfTransport { get; set; }
        public string Name { get; set; }
        public int? CarrierNumber { get; set; }
        public CarrierStatus Status { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new CarrierValidation().ValidateAndThrow(this);
        }
        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        public void MarkAuditFieldStatus(bool isUpdating = false)
        {
            if (!isUpdating)
            {
                FieldStatus[nameof(CreatedBy)] = FieldDeserializationStatus.HasValue;

                FieldStatus[nameof(CreatedDate)] = FieldDeserializationStatus.HasValue;
            }

            FieldStatus[nameof(UpdatedBy)] = FieldDeserializationStatus.HasValue;

            FieldStatus[nameof(UpdatedDate)] = FieldDeserializationStatus.HasValue;
        }
    }
}
