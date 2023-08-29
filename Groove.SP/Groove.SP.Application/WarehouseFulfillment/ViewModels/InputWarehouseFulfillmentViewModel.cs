using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class InputWarehouseFulfillmentViewModel : ViewModelBase<POFulfillmentModel>
    {
        public long Id { get; set; }

        public string CustomerPrefix { get; set; }

        public bool UpdateOrganizationPreferences { get; set; }

        public string Number { get; set; }

        public string Owner { get; set; }

        public POFulfillmentStatus Status { get; set; }

        public POFulfillmentStage Stage { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public bool IsContainDangerousGoods { get; set; }

        public string Remarks { get; set; }

        public bool IsRejected { get; set; }

        public bool IsForwarderBookingItineraryReady { get; set; }

        public bool IsFulfilledFromPO { get; set; }

        public POType? FulfilledFromPOType { get; set; }

        public FulfillmentType FulfillmentType { get; set; }

        public string PlantNo { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualTimeArrival { get; set; }
        public string ContainerNo { get; set; }
        public string HAWBNo { get; set; }
        public string NameofInternationalAccount { get; set; }
        public string CompanyNo { get; set; }
        public DateTime? ETDOrigin { get; set; }
        public DateTime? ETADestination { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public IncotermType Incoterm { get; set; }
        public string DeliveryMode { get; set; }

        public string Forwarder { get; set; }

        // LocationId
        public long ShipFrom { get; set; }

        // LocationDescription
        public string ShipFromName { get; set; }

        public ICollection<WarehouseFulfillmentAttachmentViewModel> Attachments { get; set; }
        public ICollection<WarehouseFulfillmentContactViewModel> Contacts { get; set; }
        public ICollection<WarehouseFulfillmentOrderViewModel> Orders { get; set; }

        /// <summary>
        /// To send mail back to vendor
        /// </summary>
        public BuyerComplianceModel BuyerCompliance { get; set; }

        /// <summary>
        /// To send mail back to vendor
        /// </summary>
        public Organization PrincipalOrganization { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
