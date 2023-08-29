using Groove.SP.Core.Models;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class ReportingMetricOceanVolumeByMovementViewModel
    {
        public string Category { get; set; }
        public decimal ThisWeekTotal { get; set; }
        public decimal LastWeekTotal { get; set; }
        public string Unit => Category == "CFS/CFS" ? AppConstant.CUBIC_METER : AppConstant.TWENTYFOOT_EQUIVALENT_UNIT;
    }
}