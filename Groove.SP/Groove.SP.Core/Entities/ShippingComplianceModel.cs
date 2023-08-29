using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class ShippingComplianceModel : Entity
    {
        public long Id { get; set; }

        public long BuyerComplianceId { get; set; }

        public BuyerComplianceModel BuyerCompliance { get; set; }

        public bool AllowPartialShipment { get; set; }

        public bool AllowMixedCarton { get; set; }

        public AllowMixedPackType AllowMixedPack { get; set; }

        public bool AllowMultiplePOPerFulfillment { get; set; }

    }
}