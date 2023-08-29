using System;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class ReportingMetricPOViewModel
    {
        public long ThisWeekTotalPOs { get; set; }
        public long LastWeekTotalPOs { get; set; }
        public DateTime ThisWeekStartDate { get; set; }
        public DateTime LastWeekStartDate { get; set; }
        public DateTime NextWeekStartDate { get; set; }
    }
}
