namespace Groove.CSFE.Core.Entities
{
    public class WarehouseAssignmentModel : Entity
    {
        public long WarehouseLocationId { get; set; }
        public long OrganizationId { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public OrganizationModel Organization { get; set; }
        public WarehouseLocationModel WarehouseLocation { get; set; }
    }
}