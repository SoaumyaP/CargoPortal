using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class WeeklyReportingMetricOceanVolumeViewModel
    {
        public DateTime ThisWeekStartDate { get; set; }
        public DateTime NextWeekStartDate { get; set; }
        public IList<ReportingMetricOceanVolumeByMovementViewModel> ReportingMetricOceanVolumeByMovement { get; set; }
    }
}