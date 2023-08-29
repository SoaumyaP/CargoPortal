namespace Groove.CSFE.Core.Entities
{
    public class CustomerRelationshipModel : Entity
    {
        public long SupplierId { get; set; }

        public long CustomerId { get; set; }

        public string CustomerRefId { get; set; }

        public OrganizationModel Supplier { get; set; }

        public OrganizationModel Customer { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public bool IsConfirmConnectionType { get; set; }
    }
}
