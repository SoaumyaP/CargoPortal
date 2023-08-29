using System;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class POFulfillmentItineraryModel : Entity
    {
        public long Id { get; set; }

        public long POFulfillmentId { get; set; }

        public POFulfillmentModel PoFulfillment { get; set; }

        public int Sequence { get; set; }

        public ModeOfTransportType ModeOfTransport { get; set; }

        public long? CarrierId { get; set; }

        public string CarrierName { get; set; }

        public string VesselFlight { get; set; }

        public long LoadingPortId { get; set; }

        public string LoadingPort { get; set; }

        public long DischargePortId { get; set; }

        public string DischargePort { get; set; }

        public DateTime? ETDDate { get; set; }

        public DateTime? ETADate { get; set; }

        public POFulfillmentItinerayStatus Status { get; set; }
    }
}
