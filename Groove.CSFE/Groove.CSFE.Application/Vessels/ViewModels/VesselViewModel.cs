using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Converters;
using Groove.CSFE.Application.Converters.Interfaces;
using Groove.CSFE.Application.Vessels.Validations;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Groove.CSFE.Application.Vessels.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class VesselViewModel : ViewModelBase<VesselModel>, IHasFieldStatus
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public bool? IsRealVessel { get; set; }

        public VesselStatus? Status { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new VesselValidation().ValidateAndThrow(this);
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
