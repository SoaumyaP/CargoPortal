using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class BuyerComplianceQueryModel
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string Name { get; set; }
        public string OrganizationName { get; set; }
        public BuyerComplianceStatus Status { get; set; }
        public string StatusName { get; set; }
        public BuyerComplianceStage Stage { get; set; }
        public string StageName { get; set; }
    }
}
