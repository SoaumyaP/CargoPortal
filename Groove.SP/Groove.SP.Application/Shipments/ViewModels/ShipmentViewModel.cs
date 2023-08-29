using System;
using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentBillOfLading.ViewModels;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Application.Shipments.Validations;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Newtonsoft.Json;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Application.ContractMaster.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;

namespace Groove.SP.Application.Shipments.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ShipmentViewModel : ViewModelBase<ShipmentModel>, IHasFieldStatus, IHasViewSetting
    {
        public ShipmentViewModel()
           : base()
        {
            BillOfLadingNos = new List<Tuple<long, string>>();
            MasterBillNos = new List<Tuple<long, string>>();
        }

        public long Id { get; set; }

        public string ShipmentNo { get; set; }

        public string BuyerCode { get; set; }

        public string CustomerReferenceNo { get; set; }

        public string AgentReferenceNo { get; set; }

        public string ShipperReferenceNo { get; set; }

        public string ModeOfTransport { get; set; }

        public DateTime CargoReadyDate { get; set; }

        public DateTime BookingDate { get; set; }

        public string ShipmentType { get; set; }

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime? ShipToETADate { get; set; }

        public string Movement { get; set; }

        public decimal? TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal? TotalUnit { get; set; }

        public string TotalUnitUOM { get; set; }

        public decimal? TotalGrossWeight { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public decimal? TotalNetWeight { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public decimal? TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public string ServiceType { get; set; }

        public string Incoterm { get; set; }

        public string Status { get; set; }

        public bool IsFCL { get; set; }

        public long? FulfillmentId { get; set; }

        public string FulfillmentNumber { get; set; }

        public FulfillmentType FulfillmentType { get; set; }

        public string BookingReferenceNo { get; set; }

        public POFulfillmentStage FulfillmentStage { get; set; }

        public bool IsItineraryConfirmed { get; set; }

        public string CYEmptyPickupTerminalCode { get; set; }

        public string CYEmptyPickupTerminalDescription { get; set; }

        public string CFSWarehouseCode { get; set; }

        public string CFSWarehouseDescription { get; set; }

        public DateTime? CYClosingDate { get; set; }

        public DateTime? CFSClosingDate { get; set; }

        public string CommercialInvoiceNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [NotMapped]
        public IEnumerable<Tuple<long, string>> BillOfLadingNos { get; set; }

        [NotMapped]
        public IEnumerable<Tuple<long, string>> MasterBillNos { get; set; }

        /// <summary>
        /// From buyer compliance
        /// </summary>
        [NotMapped]
        public bool? EnforceCommercialInvoiceFormat { get; set; }

        public string BookingReferenceNumber { get; set; }

        public string OrderType { get; set; }
        public string ExecutionAgentName { get; set; }
        public string ShipperName { get; set; }
        public string ConsigneeName { get; set; }
        public string LastestActivity { get; set; }
        public int IsConfirmContainer { get; set; }
        public int IsConfirmConsolidation { get; set; }

        public string LatestMilestone
        {
            get
            {
                var activity = this.Activities?.OrderByDescending(x => x.ActivityDate)
                    .FirstOrDefault(x => x.ActivityType == ActivityType.ShipmentMilestone);
                return activity?.ActivityDescription;
            }
        }

        public string Shipper => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Shipper)?.CompanyName ?? string.Empty;

        public string OriginAgent => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.OriginAgent)?.CompanyName ?? string.Empty;

        public string DestinationAgent => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.DestinationAgent)?.CompanyName ?? string.Empty;

        public string Consignee => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee)?.CompanyName ?? string.Empty;

        public string Nominee => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Principal)?.CompanyName ?? string.Empty;

        public ICollection<ShipmentContactViewModel> Contacts { get; set; }

        public ICollection<ActivityViewModel> Activities { get; set; }

        public ICollection<ShipmentBillOfLadingViewModel> ShipmentBillOfLadings { get; set; }

        public ICollection<ShipmentItemViewModel> ShipmentItems { get; set; }

        public ICollection<ConsignmentModel> Consignments { get; set; }

        public ICollection<ShipmentLoadDetailViewModel> ShipmentLoadDetails { get; set; }
        public ICollection<ConsolidationViewModel> Consolidations { get; set; }

        public decimal? BookedQuantity { get; set; }

        /// <summary>
        /// To map with <see cref="ContractMasterModel.CarrierContractNo"/> then show <see cref="ContractMasterModel.RealContractNo"/>
        /// </summary>
        public string CarrierContractNo { get; set; }
        public ContractMasterViewModel ContractMaster { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ShipmentValidation(isUpdating).ValidateAndThrow(this);
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        [JsonIgnore]
        public string ViewSettingModuleId { get; set; }

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }
    }
}
