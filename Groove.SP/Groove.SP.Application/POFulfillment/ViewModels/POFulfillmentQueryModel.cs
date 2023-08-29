using System;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class POFulfillmentQueryModel
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public POFulfillmentStatus Status { get; set; }

        public string StatusName { get; set; }

        public POFulfillmentStage Stage { get; set; }

        public string StageName { get; set; }

        public FulfillmentType FulfillmentType { get; set; }

        public OrderFulfillmentPolicy OrderFulfillmentPolicy { get; set; }

        public DateTime? BookingDate { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public string ShipFromName { get; set; }

        public string Customer { get; set; }

        public string Supplier { get; set; }

        public bool IsRejected { get; set; }

        public bool IsPending { get; set; }

        public bool IsPOAdhocChanged { get; set; }


        public DateTime CreatedDate { get; set; }
    }
}