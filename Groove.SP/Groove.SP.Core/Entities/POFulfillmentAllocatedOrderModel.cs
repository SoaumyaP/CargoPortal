namespace Groove.SP.Core.Entities
{
    public class POFulfillmentAllocatedOrderModel : Entity
    {
        public long Id { get; set; }

        public long PurchaseOrderId { get; set; }

        public long POLineItemId { get; set; }

        public long? POFulfillmentBookingRequestId { get; set; }

        public long POFulfillmentId { get; set; }

        public long? ShipmentId { get; set; }

        public int BookedQty { get; set; }

        public int BalanceQty { get; set; }

        public int? BookedPackage { get; set; }

        public decimal? Volume { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        /// <summary>
        /// Data format from ediSON: PurchaseOrders.PONumber~POLineItems.ProductCode~POLineItems.LineOrder
        /// </summary>
        public string ProductNumber { get; set; }

        public ShipmentModel Shipment { get; set; }

        public POFulfillmentBookingRequestModel POFulfillmentBookingRequest { get; set; }

        public POLineItemModel POLineItem { get; set; }

        public POFulfillmentModel POFulfillment { get; set; }
    }
}