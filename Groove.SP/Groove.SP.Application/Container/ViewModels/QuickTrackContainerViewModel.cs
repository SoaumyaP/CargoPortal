using System;
using System.Collections.Generic;
using Groove.SP.Application.QuickTrack;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Container.ViewModels
{
    public class QuickTrackContainerViewModel
    {
        public QuickTrackContainerViewModel()
        {
            Activities = new List<QuickTrackActivityViewModel>();
        }

        public string ContainerNo { get; set; }

        public string ContainerType { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public DateTime ShipToETADate { get; set; }

        public decimal TotalGrossWeight { get; set; }

        public decimal TotalNetWeight { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public decimal TotalPackage { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public string TotalPackageUOM { get; set; }

        public string TotalVolumeUOM { get; set; }

        public virtual ICollection<QuickTrackMilestone> Milestones { get; set; }

        public virtual ICollection<QuickTrackActivityViewModel> Activities { get; set; }
    }
}
