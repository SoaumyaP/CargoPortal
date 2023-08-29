using System;

namespace Groove.SP.Core.Entities
{
    public class MasterBillOfLadingQueryModel
    {
        public long Id { get; set; }

        public string MasterBillOfLadingNo { get; set; }

        public string CarrierName { get; set; }

        public string RealContractNo { get; set; }

        public string ContractType { get; set; }

        public string ContractHolder { get; set; }

        public DateTime OnBoardDate { get; set; }
    }
}
