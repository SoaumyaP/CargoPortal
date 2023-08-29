using System;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class POFulfillmentBookingRequestModel : Entity
    {
        public long Id { get; set; }
        public string BookingReferenceNumber { get; set; }
        public long POFulfillmentId { get; set; }
        public DateTime BookedDate { get; set; }
        public string RequestContent { get; set; }
        public POFulfillmentBookingRequestStatus Status { get; set; }

        /// <summary>
        /// Shipment Number from ediSON
        /// </summary>
        public string SONumber { get; set; }
        public string BillOfLadingHeader { get; set; }
        public string Warehouse { get; set; }
        public DateTime? CargoClosingDate { get; set; }
        public string CYEmptyPickupTerminalCode { get; set; }
        public string CYEmptyPickupTerminalDescription { get; set; }
        public string CFSWarehouseCode { get; set; }
        public string CFSWarehouseDescription { get; set; }
        public DateTime? CYClosingDate { get; set; }
        public DateTime? CFSClosingDate { get; set; }

        public POFulfillmentModel POFulfillment { get; set; }
       
    }
}
