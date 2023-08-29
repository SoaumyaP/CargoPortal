using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities;
using FluentValidation;
using Groove.SP.Application.CargoDetail.Validations;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.CargoDetail.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class CargoDetailViewModel : ViewModelBase<CargoDetailModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public long ShipmentId { get; set; }

        public int? Sequence { get; set; }

        public string ShippingMarks { get; set; }

        public string Description { get; set; }

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

        public decimal? ChargeableWeight { get; set; }

        public string ChargeableWeightUOM { get; set; }

        public decimal? VolumetricWeight { get; set; }

        public string VolumetricWeightUOM { get; set; }

        public string Commodity { get; set; }

        public string HSCode { get; set; }

        public string ProductNumber { get; set; }

        public string CountryOfOrigin { get; set; }

        public string OrderType { get; set; }

        /// <summary>
        /// Depend on OrderType, it will link to Freight(dbo.PurchaseOrders) or Cruise (cruise.CruiseOrders)
        /// </summary>
        public long? OrderId { get; set; }

        /// <summary>
        /// Depend on OrderType, if Cruise, it will link to Cruise Item (cruise.CruiseOrderItems)
        /// </summary>
        public long? ItemId { get; set; }

        public ShipmentViewModel Shipment { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new CargoDetailValidation(isUpdating).ValidateAndThrow(this);
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
