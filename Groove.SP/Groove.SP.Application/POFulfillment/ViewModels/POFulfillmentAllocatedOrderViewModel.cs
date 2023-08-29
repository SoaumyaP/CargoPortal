using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class POFulfillmentAllocatedOrderViewModel
    {
        public long Id { get; set; }

        public long PurchaseOrderId { get; set; }

        public long POLineItemId { get; set; }

        public long? POFulfillmentBookingRequestId { get; set; }

        public long POFulfillmentId { get; set; }

        public long? ShipmentId { get; set; }

        public int OrderedQty { get; set; }

        public int BookedQty { get; set; }

        public int BalanceQty { get; set; }

        public int LoadedQty { get; set; }

        public int OpenQty { get; set; }

        public int? BookedPackage { get; set; }

        public decimal? Volume { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        public string ShipTo { get; set; }

        public long ShipToId { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int ContainerType { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        /// <summary>
        /// It is to link to shipment, not stored into database
        /// </summary>
        public string ShipToLocationDescription { get; set; }

        /// <summary>
        /// Data format from ediSON: PurchaseOrders.PONumber~POLineItems.ProductCode~POLineItems.LineOrder
        /// </summary>
        public string ProductNumber { get; set; }
    }
}
