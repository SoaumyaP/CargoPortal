namespace Groove.SP.Core.Entities
{
    public class PurchaseOrderContactModel : Entity
    {
        public long Id { get; set; }

        public long PurchaseOrderId { get; set; }

        public long OrganizationId { get; set; }

        public string OrganizationCode { get; set; }

        public string OrganizationRole { get; set; }

        public string CompanyName { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string Department { get; set; }

        public string ContactName { get; set; }

        public string Name { get; set; }

        public string ContactNumber { get; set; }

        public string ContactEmail { get; set; }

        public string References { get; set; }

        public virtual PurchaseOrderModel PurchaseOrder { get; set; }
    }
}
