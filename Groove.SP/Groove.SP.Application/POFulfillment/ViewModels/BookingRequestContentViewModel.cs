using System;
using System.Collections.Generic;
using Groove.SP.Application.POFulfillmentContact.ViewModels;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class BookingRequestContentViewModel
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public string Owner { get; set; }

        public POFulfillmentStatus Status { get; set; }

        public POFulfillmentStage Stage { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public IncotermType Incoterm { get; set; }

        public bool IsPartialShipment { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ModeOfTransportType ModeOfTransport { get; set; }

        public long PreferredCarrier { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogisticsServiceType LogisticsService { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MovementType MovementType { get; set; }

        // LocationId
        public long ShipFrom { get; set; }

        // LocationId
        public long ShipTo { get; set; }

        public string ShipFromName { get; set; }

        public string ShipToName { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public string Remarks { get; set; }

        public bool IsForwarderBookingItineraryReady { get; set; }

        public ICollection<POFulfillmentContactViewModel> Contacts { get; set; }

        public ICollection<POFulfillmentOrderViewModel> Orders { get; set; }

        public ICollection<POFulfillmentLoadViewModel> Loads { get; set; }

        public ICollection<POFulfillmentCargoDetailViewModel> CargoDetails { get; set; }

        public ICollection<POFulfillmentItineraryViewModel> Itineraries { get; set; }
        public string PoRemark { get; set; }
    }
}