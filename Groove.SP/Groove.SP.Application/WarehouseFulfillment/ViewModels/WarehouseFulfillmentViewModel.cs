using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class WarehouseFulfillmentViewModel : ViewModelBase<POFulfillmentModel>
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public string Owner { get; set; }

        public POFulfillmentStatus Status { get; set; }

        public string StatusName => EnumHelper<POFulfillmentStatus>.GetDisplayName(Status);

        public POFulfillmentStage Stage { get; set; }

        public string StageName => EnumHelper<POFulfillmentStage>.GetDisplayName(Stage);

        public DateTime? CargoReadyDate { get; set; }

        public string Remarks { get; set; }

        public bool IsRejected { get; set; }

        public bool IsFulfilledFromPO { get; set; }

        public POType? FulfilledFromPOType { get; set; }

        public FulfillmentType FulfillmentType { get; set; }

        public string PlantNo { get; set; }

        public string DeliveryMode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public IncotermType Incoterm { get; set; }

        public DateTime? ActualTimeArrival { get; set; }

        public string ContainerNo { get; set; }

        public string HAWBNo { get; set; }

        public string NameofInternationalAccount { get; set; }

        public string CompanyNo { get; set; }

        public string Forwarder { get; set; }

        public DateTime? ETDOrigin { get; set; }

        public DateTime? ETADestination { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        // LocationDescription
        public string ShipFromName { get; set; }

        public string ModeOfTransport
        {
            get
            {
                return string.Empty;
            }
        }

        public string EmailSubject { get; set; }

        #region WarehouseLocation info
        public string ConfirmBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ConfirmedHubArrivalDate { get; set; }
        public string LoadingBay { set; get; }
        public string LocationName { get; set; }
        public string SoNo { get; set; }
        public string LocationCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPerson { get; set; }
        public string Time { get; set; }
        #endregion WarehouseLocation info

        public ICollection<AttachmentViewModel> Attachments { get; set; }

        public ICollection<WarehouseFulfillmentContactViewModel> Contacts { get; set; }

        public ICollection<BuyerApprovalViewModel> BuyerApprovals { get; set; }

        public ICollection<WarehouseFulfillmentOrderViewModel> Orders { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
