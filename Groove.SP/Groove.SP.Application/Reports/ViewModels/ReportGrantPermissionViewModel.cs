using System;

namespace Groove.SP.Application.Reports.ViewModels
{
    public class ReportGrantPermissionViewModel
    {
        public long ReportId { get; set; }
        public string OrganizationIds { get; set; }
        public bool GrantInternal { get; set; }
        public bool GrantPrincipal { get; set; }
        public bool GrantShipper { get; set; }
        public bool GrantAgent { get; set; }
        public bool GrantWarehouse { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public ReportGrantPermissionViewModel()
        {
            GrantInternal = true;

        }
        public ReportGrantPermissionViewModel(long reportId)
        {
            ReportId = reportId;
            GrantInternal = true;
        }
    }
}
