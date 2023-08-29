using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.BillOfLadingShipmentLoad.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Application.Container.Validations;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.Container.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ContainerViewModel : ViewModelBase<ContainerModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public string LoadPlanRefNo { get; set; }

        public string ContainerType { get; set; }

        public string ContainerTypeName => EnumHelper<EquipmentType>.GetEnumDescriptionByShortName(ContainerType);

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime ShipToETADate { get; set; }

        public DateTime? LoadingDate { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public string CarrierSONo { get; set; }

        public string Movement { get; set; }

        public decimal TotalGrossWeight { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public decimal TotalNetWeight { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public int TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public bool IsFCL { get; set; }

        /// <summary>
        /// Using for LCL container to check if its consolidation has been confirmed or not.
        /// Is always true if it is a FCL container.
        /// </summary>
        public bool IsConfirmed { get; set; }

        public string ShipmentOriginAgent
        {
            get
            {
                return IsFCL ? ShipmentLoads?.FirstOrDefault()?.Shipment?.OriginAgent : Consolidation?.ShipmentLoads?.FirstOrDefault()?.Shipment?.OriginAgent;
            }
        }

        public string ShipmentDestinationAgent
        {
            get
            {
                return IsFCL ? ShipmentLoads?.FirstOrDefault()?.Shipment?.DestinationAgent : Consolidation?.ShipmentLoads?.FirstOrDefault()?.Shipment?.DestinationAgent;
            }
        }


        public ICollection<AttachmentViewModel> Attachments { get; set; }

        public ICollection<ActivityViewModel> Activities { get; set; }

        public ConsolidationViewModel Consolidation { get; set; }

        public ICollection<ShipmentLoadViewModel> ShipmentLoads { get; set; }

        public ICollection<ShipmentLoadDetailViewModel> ShipmentLoadDetails { get; set; }

        public ICollection<BillOfLadingShipmentLoadViewModel> BillOfLadingShipmentLoads { get; set; }

        public string LatestActivity
        {
            get
            {
                var activity = this.Activities?.OrderByDescending(x => x.ActivityDate).FirstOrDefault();
                return activity?.ActivityDescription;
            }
        }
        
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ContainerValidation(isUpdating).ValidateAndThrow(this);
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            if (FieldStatus == null)
            {
                return true;
            }
            return FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
