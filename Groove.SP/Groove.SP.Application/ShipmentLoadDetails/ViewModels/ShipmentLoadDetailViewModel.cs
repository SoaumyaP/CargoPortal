using System;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.ShipmentLoadDetails.Validations;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.SP.Application.ShipmentLoadDetails.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ShipmentLoadDetailViewModel: ViewModelBase<ShipmentLoadDetailModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public long? ShipmentId { get; set; }

        public long? ConsignmentId { get; set; }

        public long? CargoDetailId { get; set; }

        public long? ShipmentLoadId { get; set; }

        public long? ContainerId { get; set; }

        public long? ConsolidationId { get; set; }

        public decimal? Package { get; set; }

        public string PackageUOM { get; set; }

        public decimal? Unit { get; set; }

        public string UnitUOM { get; set; }

        public decimal? Volume { get; set; }

        public string VolumeUOM { get; set; }

        public decimal? GrossWeight { get; set; }

        public string GrossWeightUOM { get; set; }

        public decimal? NetWeight { get; set; }

        public string NetWeightUOM { get; set; }

        public int? Sequence { get; set; }

        public CargoDetailViewModel CargoDetail { get; set; }

        public ShipmentViewModel Shipment { get; set; }

        [NotMapped]
        public IEnumerable<Tuple<long, string>> BillOfLadingNos { get; set; }

        public ShipmentLoadDetailViewModel()
            : base()
        { }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ShipmentLoadDetailValidation(isUpdating).ValidateAndThrow(this);
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
