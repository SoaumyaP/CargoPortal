using System.Collections.Generic;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class ComplianceSelectionModel : Entity
    {
        public long Id { get; set; }

        public long BuyerComplianceId { get; set; }

        public virtual BuyerComplianceModel BuyerCompliance { get; set; }

        public ModeOfTransportType ModeOfTransportSelections { get; set; }

        public CommodityType CommoditySelections { get; set; }

        public string ShipFromLocationSelections { get; set; }

        public string ShipToLocationSelections { get; set; }

        public MovementType MovementTypeSelections { get; set; }

        public IncotermType IncotermSelections { get; set; }

        public string CarrierSelections { get; set; }

        public FulfillmentAccuracyType FulfillmentAccuracies { get; set; }

        public string CarrierSelectionNotes { get; set; }

        public LogisticsServiceType LogisticsServiceSelections { get; set; }
    }
}