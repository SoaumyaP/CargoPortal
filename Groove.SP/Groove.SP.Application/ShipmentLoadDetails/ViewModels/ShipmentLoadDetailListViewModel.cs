using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.SP.Application.ShipmentLoadDetails.ViewModels
{
    public class ShipmentLoadDetailListViewModel
    {
        public long Id { get; set; }

        public long? ShipmentId { get; set; }

        public long? CargoDetailId { get; set; }

        public long? ShipmentLoadId { get; set; }

        public long? ContainerId { get; set; }

        public decimal? Package { get; set; }

        public string PackageUOM { get; set; }

        public decimal? Volume { get; set; }

        public string VolumeUOM { get; set; }

        public decimal? GrossWeight { get; set; }

        public string GrossWeightUOM { get; set; }

        public int? Sequence { get; set; }

        public CargoDetailListViewModel CargoDetail { get; set; }

        public ShipmentViewModel Shipment { get; set; }

        [NotMapped]
        public IEnumerable<Tuple<long, string>> BillOfLadingNos { get; set; }
    }
}
