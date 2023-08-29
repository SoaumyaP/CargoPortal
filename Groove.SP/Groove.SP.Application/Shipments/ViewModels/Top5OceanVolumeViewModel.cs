using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class Top5OceanVolumeViewModel
    {
        public DateTime ThisMonthStartDate { get; set; }
        public DateTime LastMonthStartDate { get; set; }
        public List<string> Top5 { get; set; }
        public List<decimal> TEUs { get; set; }
    }
}
