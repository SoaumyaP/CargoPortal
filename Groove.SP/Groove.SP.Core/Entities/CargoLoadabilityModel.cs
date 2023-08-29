using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class CargoLoadabilityModel : Entity
    {
        public long Id { get; set; }

        public long BuyerComplianceId { get; set; }

        public BuyerComplianceModel BuyerCompliance { get; set; }

        public string Name { get; set; }

        public EquipmentType EquipmentType { get; set; }

        public decimal CyMinimumCBM { get; set; }

        public decimal CyMaximumCBM { get; set; }

        public decimal CfsMinimumCBM { get; set; }

        public decimal CfsMaximumCBM { get; set; }
    }
}