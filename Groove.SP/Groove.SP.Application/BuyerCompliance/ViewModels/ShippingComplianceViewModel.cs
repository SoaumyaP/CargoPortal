using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class ShippingComplianceViewModel : ViewModelBase<ShippingComplianceModel>
    {
        public long Id { get; set; }

        public bool AllowPartialShipment { get; set; }

        public AllowMixedPackType AllowMixedPack { get; set; }

        public bool AllowMixedCarton { get; set; }

        public bool AllowMultiplePOPerFulfillment { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
