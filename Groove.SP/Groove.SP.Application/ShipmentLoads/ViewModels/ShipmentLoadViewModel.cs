using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Common;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities;
using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.ShipmentLoads.Validations;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Groove.SP.Application.Consignment.ViewModels;

namespace Groove.SP.Application.ShipmentLoads.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ShipmentLoadViewModel : ViewModelBase<ShipmentLoadModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public long? ShipmentId { get; set; }

        public long? ConsignmentId { get; set; }

        public long? ConsolidationId { get; set; }

        public long? ContainerId { get; set; }

        public string ModeOfTransport { get; set; }

        public string CarrierBookingNo { get; set; }

        public bool IsFCL { get; set; }

        public string LoadingPlace { get; set; }

        // OrganizationId
        public long? LoadingPartyId { get; set; }

        public string EquipmentType { get; set; }

        public SimpleShipmentViewModel Shipment { get; set; }

        public ConsignmentViewModel Consignment { get; set; }

        public ShipmentLoadViewModel()
            : base()
        { }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ShipmentLoadValidation(isUpdating).ValidateAndThrow(this);
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
