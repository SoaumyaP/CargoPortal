using Groove.SP.Core.Models;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class CargoDetailModel : Entity
    {
        public long Id { get; set; }

        public long ShipmentId { get; set; }

        public int Sequence { get; set; }

        public string ShippingMarks { get; set; }

        public string Description { get; set; }

        public decimal Unit { get; set; }

        public string UnitUOM { get; set; }

        public decimal Package { get; set; }

        public string PackageUOM { get; set; }

        public decimal Volume { get; set; }

        public string VolumeUOM { get; set; }

        public decimal GrossWeight { get; set; }

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

        public OrderType OrderType { get; set; }

        /// <summary>
        /// Depend on OrderType, it will link to Freight(dbo.PurchaseOrders) or Cruise (cruise.CruiseOrders)
        /// </summary>
        public long? OrderId { get; set; }

        /// <summary>
        /// Depend on OrderType, if Cruise, it will link to Cruise Item (cruise.CruiseOrderItems)
        /// </summary>
        public long? ItemId { get; set; }

        public virtual ShipmentModel Shipment { get; set; }

        public virtual ICollection<ShipmentLoadDetailModel> ShipmentLoadDetails { get; set; }
    }
}
