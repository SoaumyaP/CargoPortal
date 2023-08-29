using System;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class ReportingMetricShipmentViewModel
    {
        public decimal ThisWeekTotal { get; set; }
        public decimal LastWeekTotal { get; set; }
        public DateTime ThisWeekStartDate { get; set; }
        public DateTime LastWeekStartDate { get; set; }
        public DateTime NextWeekStartDate { get; set; }
    }
}