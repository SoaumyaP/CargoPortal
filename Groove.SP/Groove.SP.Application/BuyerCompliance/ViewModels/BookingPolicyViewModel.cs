using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Collections.Generic;
using FluentValidation;
using Groove.SP.Application.BuyerCompliance.Validations;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class BookingPolicyViewModel : ViewModelBase<BookingPolicyModel>
    {
        public string Name { get; set; }

        public IList<ModeOfTransportType> ModeOfTransportIds { get; set; }

        public IList<IncotermType> IncotermTypeIds { get; set; }

        public IList<string> ShipFromIds { get; set; }

        public IList<string> ShipToIds { get; set; }

        public IList<FulfillmentAccuracyType> FulfillmentAccuracyIds { get; set; }

        public IList<BookingTimelessType> BookingTimelessIds { get; set; }
 
        public IList<LogisticsServiceType> LogisticsServiceSelectionIds { get; set; }

        public IList<MovementType> MovementTypeIds { get; set; }

        public IList<string> CarrierIds { get; set; }

        public IList<CargoLoadabilityType> CargoLoadabilityIds { get; set; }

        public ApproverSettingType ApproverSetting { get; set; }

        public string ApproverUser { get; set; }

        public ValidationResultType Action { get; set; }

        public ItineraryIsEmptyType? ItineraryIsEmpty { get; set; }

        public int Order { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new BookingPolicyValidator().ValidateAndThrow(this);
        }
    }
}
