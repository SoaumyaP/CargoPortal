using System;
using System.Collections.Generic;
using Groove.SP.Core.Models;
using Groove.SP.Application.PurchaseOrderContact.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class BookingPOViewModel
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public string ModeOfTransport { get; set; }
        public string ShipFrom { get; set; }
        public long? ShipFromId { get; set; }
        public string ShipTo { get; set; }
        public long? ShipToId { get; set; }
        public string Incoterm { get; set; }
        public string CarrierCode { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ExpectedShipDate { get; set; }
        public string CarrierName { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public POType POType { get; set; }
        public PurchaseOrderStatus Status { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EquipmentType? ContainerType { get; set; }
        public string Shipper => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Shipper)?.CompanyName ?? string.Empty;
        public string Consignee => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee)?.CompanyName ?? string.Empty;

        public long? ShipperId => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Shipper)?.OrganizationId;
        public long? ConsigneeId => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee)?.OrganizationId;
        public ICollection<PurchaseOrderContactViewModel> Contacts { get; set; }
        public ICollection<BookingPOLineItemViewModel> LineItems { get; set; }

        public long RecordCount { get; set; }
    }
}
