using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class ProductVerificationSettingViewModel : ViewModelBase<ProductVerificationSettingModel>
    {
        public long Id { get; set; }

        public VerificationSettingType PreferredCarrierVerification { get; set; }

        public VerificationSettingType ProductCodeVerification { get; set; }

        public VerificationSettingType CommodityVerification { get; set; }

        public VerificationSettingType HsCodeVerification { get; set; }

        public VerificationSettingType CountryOfOriginVerification { get; set; }

        public bool IsRequireGrossWeight { get; set; }

        public bool IsRequireVolume { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
