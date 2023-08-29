using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.ContractMaster.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ContractMasterViewModel : ViewModelBase<ContractMasterModel>
    {
        public long Id { get; set; }
        public string CarrierContractNo { get; set; }
        private string _carrierCode;
        public string CarrierCode
        {
            get => string.IsNullOrEmpty(_carrierCode) ? string.Empty : _carrierCode;
            set => _carrierCode = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string RealContractNo { get; set; }
        public string AccountName { get; set; }
        public string ContractType { get; set; }
        public string ColoaderCode { get; set; }
        /// <summary>
        /// It contains two data types:
        /// <list type="number">
        /// <item>If it's matched with <see cref="Groove.CSFE.Core.Entities.OrganizationModel.EdisonCompanyCodeId"/>, store <see cref="Groove.CSFE.Core.Entities.OrganizationModel.Id"/> </item>
        /// <item>If it's not matched, store its own value.</item>
        /// </list>
        /// </summary>
        public string ContractHolder { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public ContractMasterStatus Status { get; set; }
        public bool? IsVIP { get; set; }
        public string Remarks { get; set; }
        public string ParentContract { get; set; }
        public string CustomerContractType { get; set; }        

        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
