using System;
using System.Collections.Generic;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.QuickTrack;

namespace Groove.SP.Application.BillOfLading.ViewModels
{
    public class QuickTrackBillOfLadingViewModel
    {
        public QuickTrackBillOfLadingViewModel()
        {
            Activities = new List<QuickTrackActivityViewModel>();
        }
        public string BillOfLadingNo { get; set; }
        public string JobNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public string ModeOfTransport { get; set; }
        public DateTime ShipFromETDDate { get; set; }
        public DateTime ShipToETADate { get; set; }
        public string ShipFrom { get; set; }
        public string ShipTo { get; set; }
        public string Movement { get; set; }
        public string Incoterm { get; set; }
        public string MainCarrier { get; set; }
        public string MainVessel { get; set; }
        public decimal TotalGrossWeight { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal TotalPackage { get; set; }
        public decimal TotalVolume { get; set; }
        public string TotalGrossWeightUOM { get; set; }
        public string TotalNetWeightUOM { get; set; }
        public string TotalPackageUOM { get; set; }
        public string TotalVolumeUOM { get; set; }
        public virtual ICollection<QuickTrackActivityViewModel> Activities { get; set; }
    }
}
