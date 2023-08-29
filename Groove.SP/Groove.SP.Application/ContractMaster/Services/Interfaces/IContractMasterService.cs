using Groove.SP.Application.Common;
using Groove.SP.Application.ContractMaster.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.MasterBillOfLading.Services.Interfaces
{
    public interface IContractMasterService : IServiceBase<ContractMasterModel, ContractMasterViewModel>
    {
        /// <summary>
        /// To fetch data for Carrier contract no combo box as creating new master bill of lading, via GUI
        /// </summary>
        /// <param name="searchTerm">Text to search</param>
        /// <param name="carrierCode">Carrier code/SCAC to search</param>
        /// <param name="currentDate">Current date without time</param>
        /// <param name="currentUser">Current user identity information</param>
        /// <returns>List of select options</returns>
        Task<IEnumerable<DropDownListItem<string>>> GetMasterBOLContractMasterOptionsAsync(string searchTerm, string carrierCode, DateTime currentDate, IdentityInfo currentUser);

        /// <summary>
        /// To fetch data for Carrier contract no combo box as editing shipment, via GUI
        /// </summary>
        /// <param name="searchTerm">Text to search</param>
        /// <param name="modeOfTransport">Mode of transport to search</param>
        /// <param name="currentDate">Current date without time</param>
        /// <param name="currentUser">Current user identity information</param>
        /// <returns>List of select options</returns>
        Task<IEnumerable<ContractMasterViewModel>> GetShipmentContractMasterOptionsAsync(string searchTerm, long shipmentId, DateTime currentDate, IdentityInfo currentUser);

        /// <summary>
        /// To get ContractMaster include some info in master data
        /// </summary>
        /// <param name="id">ContractMasterId</param>
        /// <returns></returns>
        Task<ContractMasterQueryModel> GetByKeyAsync(long id);

        Task<ContractMasterQueryModel> CreateAsync(ContractMasterQueryModel viewModel, string userName);
        Task<ContractMasterQueryModel> UpdateAsync(long id, ContractMasterQueryModel viewModel, string userName);

        /// <summary>
        /// To check a contract master has already existed in system by contractNo
        /// </summary>
        /// <param name="contractNo">realContractNo</param>
        /// <returns></returns>
        Task<bool> CheckContractAlreadyExistsAsync(string contractNo);

        Task UpdateStatusAsync(long id, ContractMasterStatus status, string userName);

        Task AutoExpireStatusAsync();

        /// <summary>
        /// Only apply for API, If ContractHolder = ediSONCompanyCodeId  → Store ContractHolder = OrganizationId
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        Task RemapContractHolderAsync(ContractMasterViewModel viewModel);
    }
}
