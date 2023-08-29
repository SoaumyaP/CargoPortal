using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ReportQueryModel
    {
        public long Id { get; set; }
        public string ReportName { get; set; }
        public string ReportUrl { get; set; }
        public string ReportDescription { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string ReportGroup { get; set; }

        #region To link to Telerik reporting server

        public string TelerikCategoryId { get; set; }
        public string TelerikCategoryName { get; set; }
        public string TelerikReportId { get; set; }

        #endregion To link to Telerik reporting server

    }
}
