using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ReportPermissionModel : Entity
    {
        public long Id { get; set; }
        public long ReportId { get; set; }
        public long RoleId { get; set; }
        public string OrganizationIds { get; set; }

        public virtual ReportModel Report { get; set; }
    }
}
