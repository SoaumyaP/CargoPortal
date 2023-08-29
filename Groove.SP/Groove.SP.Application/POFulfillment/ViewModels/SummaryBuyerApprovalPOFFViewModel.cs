using Groove.SP.Application.POFulfillmentContact.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class SummaryBuyerApprovalPOFFViewModel
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public string Customer => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Principal)?.CompanyName ?? string.Empty;

        public string Supplier => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Supplier)?.CompanyName ?? string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public ModeOfTransportType ModeOfTransport { get; set; }

        public string LogisticsServiceName => EnumHelper<LogisticsServiceType>.GetDisplayName(LogisticsService);

        public string MovementTypeName => EnumHelper<MovementType>.GetDisplayName(MovementType);

        [JsonConverter(typeof(StringEnumConverter))]
        public LogisticsServiceType LogisticsService { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MovementType MovementType { get; set; }

        public long ShipFrom { get; set; }

        public long ShipTo { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public ICollection<POFulfillmentContactViewModel> Contacts { get; set; }

        public IEnumerable<Tuple<long, string>> PurchaseOrderNos { get; set; }

        public POFulfillmentStage Stage { get; set; }

        public FulfillmentType FulfillmentType { get; set; }
    }
}
