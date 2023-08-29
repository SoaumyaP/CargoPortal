using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class ComplianceSelectionViewModel : ViewModelBase<ComplianceSelectionModel>
    {
        public long Id { get; set; }

        public IList<ModeOfTransportType> ModeOfTransportIds { get; set; }

        public IList<CommodityType> CommodityIds { get; set; }

        public IList<string> ShipFromLocationIds { get; set; }

        public IList<string> ShipToLocationIds { get; set; }

        public IList<MovementType> MovementTypeIds { get; set; }

        public IList<IncotermType> IncotermTypeIds { get; set; }

        public IList<string> CarrierIds { get; set; }

        public FulfillmentAccuracyType FulfillmentAccuracies { get; set; }

        public string CarrierSelectionNotes { get; set; }

        public IList<LogisticsServiceType> LogisticsServiceSelectionIds { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
