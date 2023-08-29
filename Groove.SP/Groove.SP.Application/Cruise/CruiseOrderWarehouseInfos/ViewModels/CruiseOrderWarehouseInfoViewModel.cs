using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Validations;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class CruiseOrderWarehouseInfoViewModel : ViewModelBase<CruiseOrderWarehouseInfoModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public DateTime? InDate { get; set; }

        public DateTime? AgentInDate { get; set; }

        public long? ReceivedQty { get; set; }

        public string PackageID { get; set; }

        public string OnHold { get; set; }

        public string OnHoldCode { get; set; }

        public string OSD { get; set; }

        public string OSDReason { get; set; }

        public string Hazardous { get; set; }

        public string MRStatus { get; set; }

        public DateTime? GRDate { get; set; }

        public string PackingList { get; set; }

        public DateTime? RTPDate { get; set; }

        public DateTime? PackedDate { get; set; }

        public DateTime? PLClosingDate { get; set; }

        public int? ReceivedDays { get; set; }

        public int? PhysicalQty { get; set; }

        public int? ReleaseToPackDays { get; set; }

        public int? PackedDays { get; set; }

        public int? KeepWHDays { get; set; }

        public int? OnHoldDays { get; set; }

        public int? AvailableDays { get; set; }

        public DateTime? RemoteDate { get; set; }

        public string RemoteStatus { get; set; }

        public string Bonded { get; set; }

        public string BondNo { get; set; }

        public string Warehouse { get; set; }      

        public string ModeOfTransport { get; set; }

        public string ContainerId { get; set; }

        public string ContainerNumber { get; set; }

        public string BookedToShip { get; set; }

        public string BookingNumber { get; set; }

        public string HAWBMAWB { get; set; }

        public string DeliveryTicket { get; set; }

        public string RefID { get; set; }

        public string Department { get; set; }

        public string POCause { get; set; }

        public string Ship { get; set; }

        public string ReqType { get; set; }

        public string ReqType2 { get; set; }

        public string ReqType3 { get; set; }

        public string Delivered { get; set; }

        public string UNNo { get; set; }

        public string UOM { get; set; }

        public decimal? KGS { get; set; }

        public string Dimension { get; set; }

        public decimal? CBM { get; set; }

        public long? ReqLine { get; set; }

        public long? QuantityDelivered { get; set; }

        public DateTime? ReqApprovedDate { get; set; }

        public DateTime? ExpLineShipDate { get; set; }

        public DateTime? BookingCreatedDate { get; set; }

        public DateTime? DeliveryConfirmDate { get; set; }

        public string BookingRemarks { get; set; }       

        public long CruiseOrderItemId { get; set; }

        #region Used to link warehouse to cruise order item as importing via API

        public long? CruiseOrderId { get; set; }

        public string ItemId { get; set; }

        public int? POLine { get; set; }

        #endregion Used to link warehouse to cruise order item as importing via API


        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new CruiseOrderWarehouseInfoViewModelValidator(isUpdating).ValidateAndThrow(this);
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        public void MarkAuditFieldStatus(bool isUpdating = false)
        {
            if (!isUpdating)
            {
                this.FieldStatus[nameof(this.CreatedBy)] = FieldDeserializationStatus.HasValue;
                this.FieldStatus[nameof(this.CreatedDate)] = FieldDeserializationStatus.HasValue;
            }
            this.FieldStatus[nameof(this.UpdatedBy)] = FieldDeserializationStatus.HasValue;
            this.FieldStatus[nameof(this.UpdatedDate)] = FieldDeserializationStatus.HasValue;
        }
    }
}
