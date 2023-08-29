namespace Groove.SP.Core.Entities
{
    public class RoutingOrderInvoiceModel : Entity
    {
        public long Id { get; set; }
        public long RoutingOrderId { get; set; }
        public string InvoiceType { get; set; }
        public string InvoiceNumber { get; set; }

        public virtual RoutingOrderModel RoutingOrder { get; set; }
    }
}
