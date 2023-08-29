using System;
using System.Collections.Generic;
using Groove.SP.Application.QuickTrack;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class QuickTrackShipmentViewModel
    {
        public string ShipmentNo { get; set; }

        public string ModeOfTransport { get; set; }

        public string ServiceType { get; set; }

        public string TotalGrossWeight { get; set; }

        public decimal TotalNetWeight { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public DateTime ShipToETADate { get; set; }

        public string Movement { get; set; }

        public string Incoterm { get; set; }

        public decimal TotalPackage { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalPackageUOM { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public string TotalVolumeUOM { get; set; }

        public ICollection<string> BillOfLadingNos { get; set; }

        public ICollection<QuickTrackMilestone> Milestones { get; set; }

        public ICollection<QuickTrackActivityViewModel> Activities { get; set; }
    }
}
