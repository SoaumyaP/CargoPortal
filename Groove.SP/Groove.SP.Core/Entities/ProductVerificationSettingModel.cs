using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class ProductVerificationSettingModel : Entity
    {
        public long Id { get; set; }

        public long BuyerComplianceId { get; set; }

        public BuyerComplianceModel BuyerCompliance { get; set; }

        public VerificationSettingType PreferredCarrierVerification { get; set; }

        public VerificationSettingType ProductCodeVerification { get; set; }

        public VerificationSettingType CommodityVerification { get; set; }

        public VerificationSettingType HsCodeVerification { get; set; }

        public VerificationSettingType CountryOfOriginVerification { get; set; }

        public bool IsRequireGrossWeight { get; set; }

        public bool IsRequireVolume { get; set; }
    }
}