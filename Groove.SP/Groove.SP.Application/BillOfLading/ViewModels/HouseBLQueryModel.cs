using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.BillOfLading.ViewModels
{
    public class HouseBLQueryModel
    {
        public long Id{ set; get; }
        public string BillOfLadingNo { set; get; }

        public string JobNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public string ModeOfTransport { get; set; }

        public string BillOfLadingType { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public DateTime ShipToETADate { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public string OriginAgent { get; set; }

        public string DestinationAgent { get; set; }

        public string Customer { get; set; }

        public string Movement { get; set; }

        public string Incoterm { get; set; }
        public decimal? TotalGrossWeight { get; set; }

        public decimal? TotalNetWeight { get; set; }

        public decimal? TotalPackage { get; set; }

        public decimal? TotalVolume { get; set; }
    }
}
