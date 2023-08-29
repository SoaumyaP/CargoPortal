using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class AgentAssignmentModel : Entity
    {
        public long Id { get; set; }

        public int? AutoCreateShipment { get; set; }

        public AgentType AgentType { get; set; }

        public long? CountryId { get; set; }

        public string PortSelectionIds { get; set; }

        public long AgentOrganizationId { get; set; }

        public long BuyerComplianceId { get; set; }

        public int Order { get; set; }

        public string ModeOfTransport { get; set; }

        public virtual BuyerComplianceModel BuyerCompliance { get; set; }
    }
}