using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class MonthlyReportingOceanVolumeByMovementViewModel
    {
        public DateTime LastMonthStartDate { get; set; }
        public DateTime ThisMonthStartDate { get; set; }
        public IEnumerable<ReportingPieChartViewModel> ReportingPieCharts { get; set; }
    }
}
