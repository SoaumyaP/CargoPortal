using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;


namespace Groove.SP.Application.PurchaseOrders.Services.Interfaces
{
    public interface IPurchaseOrderListService : IServiceBase<PurchaseOrderModel, PurchaseOrderListViewModel>
    {
        Task<Core.Data.DataSourceResult> ListAsync(Core.Data.DataSourceRequest request, bool isInternal, string affiliates, string supplierCustomerRelationships,
            long? delegatedOrganizationId = 0, string statisticKey = "", string statisticFilter = "", string statisticValue = "", string itemNo = "", bool isExport = false);

        /// <summary>
        /// This method to get all POs belonging to selected Principal organization
        /// It works for Internal users and Shipper only.
        /// </summary>
        /// <param name="skip">A number of records will be skipped</param>
        /// <param name="take">A number of records will be take</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="selectedPOId">Selected Po Id</param>
        /// <param name="supplierCustomerRelationships">Only available if shipper</param>
        /// <param name="principalOrganizationId">Selected principal organization</param>
        /// <param name="supplierOrganizationId">Current organization of shipper </param>
        /// /// <param name="isInternal">If it is internal user</param>
        Task<IEnumerable<SelectPurchaseOrderViewModel>> GetPurchaseOrderSelectionsByPrincipalIdAsync(int skip, int take, string searchTerm, long selectedPOId, string supplierCustomerRelationships, long principalOrganizationId, long supplierOrganizationId,int roleId, string affiliates, bool isInternal);

        Task<IEnumerable<SelectUnmappedPurchaseOrderViewModel>> GetUnmappedPurchaseOrderSelectionsAsync(int skip, int take, string searchTerm, long principalOrganizationId, long supplierOrganizationId);

        /// <summary>
        /// To get list of purchase order which already shipped from the linked shipment (departure)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isInternal"></param>
        /// <param name="affiliates"></param>
        /// <param name="supplierCustomerRelationships"></param>
        /// <param name="delegatedOrganizationId"></param>
        /// <returns></returns>
        Task<DataSourceResult> ShippedListAsync(DataSourceRequest request, bool isInternal, string affiliates, string supplierCustomerRelationships, long? delegatedOrganizationId = 0);
    }
}