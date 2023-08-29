using System;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Reports.ViewModels
{
    public class ReportViewModel : ViewModelBase<ReportModel>
    {
        public long Id { get; set; }
        public string ReportName { get; set; }
        public string ReportUrl { get; set; }
        public string ReportDescription { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string ReportGroup { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}