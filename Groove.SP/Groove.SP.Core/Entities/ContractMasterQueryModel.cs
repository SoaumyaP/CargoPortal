using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Core.Entities
{
    public class ContractMasterQueryModel
    {
        public long Id { set; get; }
        public string CarrierContractNo { get; set; }
        private string _carrierCode;
        public string CarrierCode
        {
            get => string.IsNullOrEmpty(_carrierCode) ? string.Empty : _carrierCode;
            set => _carrierCode = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string CarrierName { get; set; }
        public string RealContractNo { get; set; }
        public string ContractType { get; set; }
        public string ContractHolder { set; get; }
        public string OrganizationName { get; set; }
        public string ColoaderCode { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime ValidToDate { get; set; }
        public ContractMasterStatus Status { get; set; }
        public bool IsVip { set; get; }
        public string CustomerContractType { get; set; }
    }
}
