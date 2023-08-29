using System;
using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Common;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Attachment.ViewModels
{
    public class AttachmentViewModel : ViewModelBase<AttachmentModel>
    {
        public long Id { get; set; }

        public string FileName { get; set; }

        public long? ShipmentId { get; set; }

        public long? POFulfillmentId { get; set; }

        public long? ConsignmentId { get; set; }

        public long? ContainerId { get; set; }

        public long? BillOfLadingId { get; set; }

        public long? MasterBillOfLadingId { get; set; }

        public string AttachmentType { get; set; }

        public string BlobId { get; set; }

        public string Description { get; set; }

        public string ReferenceNo { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadedDateTime { get; set; }

        public string DocumentLevel { get; set; }

        public string Alias { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// To get entity type by related entity id.
        /// <br></br><b>It should work for all adding/updating</b>.
        /// <br></br><b>As removing, it passed GlobalId, then use <see cref="Groove.SP.Application.Utilities.GetGlobalIdParts"/> </b>
        /// </summary>
        /// <returns></returns>
        public string GetEntityType()
        {
            if (ShipmentId.HasValue && ShipmentId.Value > 0)
            {
                return EntityType.Shipment;
            }
            else if (POFulfillmentId.HasValue && POFulfillmentId.Value > 0)
            {
                return EntityType.POFullfillment;
            }
            if (ConsignmentId.HasValue && ConsignmentId.Value > 0)
            {
                return EntityType.Consignment;
            }
            if (ContainerId.HasValue && ContainerId.Value > 0)
            {
                return EntityType.Container;
            }
            if (BillOfLadingId.HasValue && BillOfLadingId.Value > 0)
            {
                return EntityType.BillOfLading;
            }
            if (MasterBillOfLadingId.HasValue && MasterBillOfLadingId.Value > 0)
            {
                return EntityType.MasterBill;
            }
            return string.Empty;
        }

        /// <summary>
        /// To get global id by related entity id.
        /// <br></br>E.x: SHI_123 (Shipment), POF_456 (Booking)
        /// </summary>
        /// <returns></returns>
        public string GetGlobalId()
        {
            string globalId = string.Empty;
            if (ShipmentId.GetValueOrDefault() > 0)
            {
                globalId = CommonHelper.GenerateGlobalId(ShipmentId.Value, EntityType.Shipment);
            }
            if (ContainerId.GetValueOrDefault() > 0)
            {
                globalId = CommonHelper.GenerateGlobalId(ContainerId.Value, EntityType.Container);
            }
            if (BillOfLadingId.GetValueOrDefault() > 0)
            {
                globalId = CommonHelper.GenerateGlobalId(BillOfLadingId.Value, EntityType.BillOfLading);
            }
            if (MasterBillOfLadingId.GetValueOrDefault() > 00)
            {
                globalId = CommonHelper.GenerateGlobalId(MasterBillOfLadingId.Value, EntityType.MasterBill);
            }
            if (POFulfillmentId.GetValueOrDefault() > 0)
            {
                globalId = CommonHelper.GenerateGlobalId(POFulfillmentId.Value, EntityType.POFullfillment);
            }
            if (ConsignmentId.GetValueOrDefault() > 0)
            {
                globalId = CommonHelper.GenerateGlobalId(ConsignmentId.Value, EntityType.Consignment);
            }
            return globalId;
        }
    }

    public class ShareAttachmentRequestModel
    {
        public IEnumerable<AttachmentModel> SelectedAttachments { get; set; }

        public IEnumerable<string> MailingList { get; set; }
    }
}
