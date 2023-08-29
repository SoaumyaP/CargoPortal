using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.BillOfLadingConsignment.Validations;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.BillOfLadingConsignment.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class BillOfLadingConsignmentViewModel : ViewModelBase<BillOfLadingConsignmentModel>, IHasFieldStatus
    {
        public long ConsignmentId { get; set; }

        public long BillOfLadingId { get; set; }

        public long? ShipmentId { get; set; }

        public ConsignmentViewModel Consignment { get; set; }

        public BillOfLadingViewModel BillOfLading { get; set; }

        public ShipmentViewModel Shipment { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new BillOfLadingConsignmentValidation(isUpdating).ValidateAndThrow(this);
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
