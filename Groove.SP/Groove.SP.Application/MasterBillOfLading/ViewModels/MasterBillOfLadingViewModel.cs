using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using Groove.SP.Application.ContractMaster.ViewModels;

namespace Groove.SP.Application.MasterBillOfLading.ViewModels
{
    public class MasterBillOfLadingViewModel : ViewModelBase<MasterBillOfLadingModel>
    {
        public long Id { get; set; }

        /// <summary>
        /// OrganizationID
        /// </summary>
        public long ExecutionAgentId { get; set; }

        public string MasterBillOfLadingNo { get; set; }

        public string MasterBillOfLadingType { get; set; }

        public string ModeOfTransport { get; set; }

        public string Movement { get; set; }

        /// <summary>
        /// To map with <see cref="ContractMasterModel.CarrierContractNo"/> then show <see cref="ContractMasterModel.RealContractNo"/>
        /// </summary>
        public string CarrierContractNo { get; set; }    

        public string CarrierName { get; set; }

        public string SCAC { get; set; }

        public string AirlineCode { get; set; }

        public string VesselFlight { get; set; }

        public string Vessel { get; set; }

        public string Voyage { get; set; }

        public string FlightNo { get; set; }

        public string PlaceOfReceipt { get; set; }

        public string PortOfLoading { get; set; }

        public string PortOfDischarge { get; set; }

        public string PlaceOfDelivery { get; set; }

        public string PlaceOfIssue { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime OnBoardDate { get; set; }

        /// <summary>
        /// To define whether it is direct master or not
        /// <list type="bullet">
        /// <item>True, link directly to Shipment -> link to Shipment/Container via Consignments</item>
        /// <item>False, link to House bill of lading -> link to Shipment/Container via BillOfLadingShipmentLoads</item>
        /// </list>
        /// </summary>
        public bool IsDirectMaster { get; set; }

        /// <summary>
        /// To link by <see cref="MasterBillOfLadingModel.CarrierContractNo"/> == <see cref="ContractMasterModel.CarrierContractNo"/>
        /// </summary>
        public ContractMasterViewModel ContractMaster { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
           
        }
    }
}
