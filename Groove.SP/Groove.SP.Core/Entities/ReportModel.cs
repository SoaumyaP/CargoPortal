using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ReportModel : Entity
    {
        public long Id { get; set; }
        public string ReportName { get; set; }
        public string ReportUrl { get; set; }
        public string ReportDescription { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string ReportGroup { get; set; }
        public string StoredProcedureName { get; set; }

        public virtual ICollection<ReportPermissionModel> Permissions { get; set; }

        /// <summary>
        /// To link Report Id on Telerik
        /// </summary>
        public string TelerikReportId { get; set; }

        /// <summary>
        /// To link Category Id on Telerik
        /// </summary>
        public string TelerikCategoryId { get; set; }

        /// <summary>
        /// To link Category Name on Telerik
        /// </summary>
        public string TelerikCategoryName { get; set; }

        
        /// <summary>
        /// Whether it applies to create scheduling
        /// </summary>
        public bool SchedulingApply { get; set; }

    }
}
