namespace Groove.SP.Infrastructure.CSFE.Models
{
    public class WarehouseAssignment
    {
        public long WarehouseLocationId { get; set; }
        public long OrganizationId { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        public WarehouseLocation WarehouseLocation { get; set; }
    }
}
