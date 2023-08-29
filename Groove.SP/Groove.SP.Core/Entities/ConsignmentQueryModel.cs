using System;

namespace Groove.SP.Core.Entities
{
    public class ConsignmentQueryModel
    {
        public long Id { get; set; }

        public DateTime? ConsignmentDate { get; set; }

        public long ShipmentId { get; set; }

        public string ShipmentNo { get; set; }

        public string ExecutionAgentName { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public string Status { get; set; }
    }
}
