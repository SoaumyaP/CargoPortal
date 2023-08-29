using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.Validations;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Groove.SP.Application.Shipments.ViewModels;

namespace Groove.SP.Application.Consignment.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ConsignmentViewModel : ViewModelBase<ConsignmentModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public long ShipmentId { get; set; }

        public string ConsignmentType { get; set; }

        public DateTime? ConsignmentDate { get; set; }

        public DateTime? ConfirmedDate { get; set; }

        // OrganizationId
        public long ExecutionAgentId { get; set; }

        public string ExecutionAgentName { get; set; }

        public string AgentReferenceNumber { get; set; }

        public string ConsignmentMasterBL { get; set; }

        public string ConsignmentHouseBL { get; set; }

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime ShipToETADate { get; set; }

        public string Status { get; set; }

        public string ModeOfTransport { get; set; }

        public string Movement { get; set; }

        public decimal? Unit { get; set; }

        public string UnitUOM { get; set; }

        public decimal? Package { get; set; }

        public string PackageUOM { get; set; }

        public decimal? Volume { get; set; }

        public string VolumeUOM { get; set; }

        public decimal? GrossWeight { get; set; }

        public string GrossWeightUOM { get; set; }

        public decimal? NetWeight { get; set; }

        public string NetWeightUOM { get; set; }

        public long? HouseBillId { get; set; }

        public long? MasterBillId { get; set; }

        public bool TriangleTradeFlag { get; set; }

        public bool MemoBOLFlag { get; set; }

        public int? Sequence { get; set; }

        public ShipmentViewModel Shipment { get; set; }

        public string ServiceType { get; set; }


        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ConsignmentValidation(isUpdating).ValidateAndThrow(this);
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
