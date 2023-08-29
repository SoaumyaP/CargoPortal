using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    public class Booking
    {
        public Booking()
        {
            Version = "1.0";
        }

        public string Version { get; set; }

        public string ShipmentNumber { get; set; }

        public string SubmissionOn { get; set; }

        public string SendID { get; set; }

        public DateTime FileCreatedOn { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BooleanOption? IsShipperPickUp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BooleanOption? IsNotifyPartyAsConsignee { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BooleanOption? IsContainDangerousGoods { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ShipmentOn { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IncotermType? Incoterm { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Term? Term { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LoadingPort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiptPort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DischargePort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeliveryPort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public MovementType? Movement { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public OceanFreightCharge? OceanFreightCharge { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PayableAt? PayableAt { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Remarks { get; set; }

        public Status Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<Container> Containers { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<Party> Parties { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<Product> Products { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string VesselVoyage { set; get; }
    }
}
