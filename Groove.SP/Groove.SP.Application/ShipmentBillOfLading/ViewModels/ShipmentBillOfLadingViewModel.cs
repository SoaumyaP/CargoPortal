using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.ShipmentBillOfLading.Validations;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.ShipmentBillOfLading.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ShipmentBillOfLadingViewModel : ViewModelBase<ShipmentBillOfLadingModel>, IHasFieldStatus
    {
        public long ShipmentId { get; set; }

        public long BillOfLadingId { get; set; }

        public BillOfLadingViewModel BillOfLading { get; set; }


        public ShipmentBillOfLadingViewModel()
            : base()
        { }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ShipmentBillOfLadingValidation(isUpdating).ValidateAndThrow(this);
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
