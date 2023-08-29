using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class BookingPolicyModel : Entity
    {
        public long Id { get; set; }

        public long BuyerComplianceId { get; set; }

        public virtual BuyerComplianceModel BuyerCompliance { get; set; }

        public string Name { get; set; }

        public ModeOfTransportType ModeOfTransports { get; set; }

        public IncotermType IncotermSelections { get; set; }

        public string ShipFromLocationSelections { get; set; }

        public string ShipToLocationSelections { get; set; }

        public FulfillmentAccuracyType FulfillmentAccuracies { get; set; }

        public BookingTimelessType BookingTimeless { get; set; }

        public LogisticsServiceType LogisticsServiceSelections { get; set; }

        public MovementType MovementTypeSelections { get; set; }

        public string CarrierSelections { get; set; }

        public CargoLoadabilityType CargoLoadabilities { get; set; }

        public ApproverSettingType ApproverSetting { get; set; }

        public string ApproverUser { get; set; }

        public ValidationResultType Action { get; set; }

        public ItineraryIsEmptyType? ItineraryIsEmpty { get; set; }

        public int Order { get; set; }
    }
}