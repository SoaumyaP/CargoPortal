using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class PurchaseOrderVerificationSettingViewModel : ViewModelBase<PurchaseOrderVerificationSettingModel>
    {
        public long Id { get; set; }

        public VerificationSettingType ExpectedShipDateVerification { get; set; }

        public VerificationSettingType ExpectedDeliveryDateVerification { get; set; }

        public VerificationSettingType ConsigneeVerification { get; set; }

        public VerificationSettingType ShipperVerification { get; set; }

        public VerificationSettingType ShipFromLocationVerification { get; set; }

        public VerificationSettingType ShipToLocationVerification { get; set; }

        public VerificationSettingType PaymentTermsVerification { get; set; }

        public VerificationSettingType PaymentCurrencyVerification { get; set; }

        public VerificationSettingType ModeOfTransportVerification { get; set; }

        public VerificationSettingType IncotermVerification { get; set; }

        public VerificationSettingType LogisticsServiceVerification { get; set; }

        public VerificationSettingType MovementTypeVerification { get; set; }

        public VerificationSettingType PreferredCarrierVerification { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
