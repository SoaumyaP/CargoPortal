using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class ContractMasterModel: Entity
    {
        public long Id { get; set; }
        /// <summary>
        /// To map with <see cref="MasterBillOfLadingModel.CarrierContractNo"/>
        /// </summary>
        public string CarrierContractNo { get; set; }
        /// <summary>
        /// To map with <see cref="Groove.CSFE.Core.Entities.CarrierModel.CarrierCode"/>
        /// </summary>
        public string CarrierCode { get; set; }
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
        public string CustomerContractType { get;set; }

    }
}
