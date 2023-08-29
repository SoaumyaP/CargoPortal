using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.BillOfLading.Validations;
using Groove.SP.Application.BillOfLadingContact.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.BillOfLading.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class BillOfLadingViewModel : ViewModelBase<BillOfLadingModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public string BillOfLadingNo { get; set; }

        public long? MasterBillOfLadingId { get; set; }

        public string MasterBillOfLadingNo { get; set; }

        /// <summary>
        /// OrganizationID
        /// </summary>
        public long? ExecutionAgentId { get; set; }

        public string BillOfLadingType { get; set; }

        public string MainCarrier { get; set; }

        public string MainVessel { get; set; }

        public decimal? TotalGrossWeight { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public decimal? TotalNetWeight { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public decimal? TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal? TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public string JobNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public string ModeOfTransport { get; set; }

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime ShipToETADate { get; set; }

        public string Movement { get; set; }

        public string Incoterm { get; set; }

        

        public string Shipper => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Shipper)?.CompanyName ?? string.Empty;

        public string OriginAgent => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.OriginAgent)?.CompanyName ?? string.Empty;

        public string DestinationAgent => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.DestinationAgent)?.CompanyName ?? string.Empty;

        public string Consignee => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee)?.CompanyName ?? string.Empty;

        public string NotifyParty => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.NotifyParty)?.CompanyName ?? string.Empty;

        public string NominationPrincipal => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Principal)?.CompanyName ?? string.Empty;



        public ICollection<BillOfLadingContactViewModel> Contacts { get; set; }


        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new BillOfLadingValidation(isUpdating).ValidateAndThrow(this);
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }        

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
