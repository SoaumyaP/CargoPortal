using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Groove.SP.Infrastructure.EBookingManagementAPI.eSI
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Booking
    {

        public Booking()
        {
            Version = "1.4";
        }

        public string Version { get; set; }

        public string ShipmentNumber { get; set; }

        public string SubmissionOn { get; set; }

        public string SendID { get; set; }

        public DateTime FileCreatedOn { get; set; }

        public BooleanOption IsShipperPickUp { get; set; }

        public BooleanOption IsNotifyPartyAsConsignee { get; set; }

        public BooleanOption IsContainDangerousGoods { get; set; }

        public string DangerousGoodsRemarks { get; set; }

        public string ShipmentOn { get; set; }

        public IncotermType? Incoterm { get; set; }

        public Term? Term { get; set; }

        public string LoadingPort { get; set; }

        public string ReceiptPort { get; set; }

        public string DischargePort { get; set; }

        public string DeliveryPort { get; set; }

        public MovementType? Movement { get; set; }

        public OceanFreightCharge? OceanFreightCharge { get; set; }

        public PayableAt? PayableAt { get; set; }

        public string Vessel { get; set; }

        public string Voyage { get; set; }

        public string MarksNos { get; set; }

        public string Remarks { get; set; }

        public EeSIStatus Status { get; set; }

        public IList<Container> ContainerInfo { get; set; }

        public IList<Party> Parties { get; set; }

        public IList<Product> Products { get; set; }
    }
}
