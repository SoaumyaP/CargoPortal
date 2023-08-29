using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.PurchaseOrderContact.ViewModels;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Hangfire;

namespace Groove.SP.Application.PurchaseOrders.Services.Interfaces
{
    public interface IPurchaseOrderService : IServiceBase<PurchaseOrderModel, PurchaseOrderViewModel>
    {
        [JobDisplayName("Import PO from Excel")]
        Task ImportFromExcel(byte[] file, string userName, long importDataProgressId);

        Task<PurchaseOrderViewModel> GetAsync(long id, IdentityInfo currentUser, string affiliates, string supplierCustomerRelationships = "", long? delegatedOrganizationId = 0, bool? replacedByOrganizationReferences = false);

        Task<IEnumerable<PurchaseOrderProgressCheckViewModel>> SearchPOsForProgressCheckAsync(string jsonFilter, IdentityInfo currentUser, string affiliates = "");
        Task<IEnumerable<POProgressCheckQueryModel>> SearchPOsForProgressCheckFromEmailAsync(long buyerComplianceId, string poIds);

        Task<IEnumerable<PurchaseOrderViewModel>> CreateAsync(IEnumerable<CreatePOViewModel> model, string userName);

        /// <summary>
        /// To import Purchase Order via API
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<PurchaseOrderViewModel> CreateAsync(CreatePOViewModel model, string userName);

        /// <summary>
        /// To update Purchase Order via API
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<PurchaseOrderViewModel> UpdateAsync(string poKey, UpdatePOViewModel model);

        Task CloseAsync(long id, UpdatePOViewModel model, string userName);

        Task AssignPOsAsync(AssignPurchaseOrdersViewModel model, string username);

        Task<IEnumerable<PurchaseOrderViewModel>> UpdatePOProgressCheckRangeAsync(IEnumerable<PurchaseOrderProgressCheckViewModel> model, IdentityInfo currentUser);

        Task UpdateContactsToLatestByOrgCodesAsync(IEnumerable<PurchaseOrderContactViewModel> contactsViewModel);

        Task DelegateAsync(long id, DelegationPOViewModel model);

        Task<IEnumerable<BookingPOViewModel>> GetCustomerPOListByIds(string purchaseOrderIds, string customerOrgCode, bool replacedByOrganizationReferences, long preferredOrganizationId);

        Task<IEnumerable<BookingPOViewModel>> GetCustomerPOListBySearching(int skip, int take, string searchType, string searchTerm, long selectedPOId, POType poType, string affiliates, long customerOrgId, string customerOrgCode, long supplierOrgId, string supplierCompanyName, IdentityInfo currentUser, bool replacedByOrganizationReferences);

        Task<ShipmentPOLineItemViewModel> GetPOLineItem(long purchaseOrderId, long poLineItemId);

        Task<POLineItemArticleMasterViewModel> GetInformationFromArticleMaster(long purchaseOrderId, string productCode);

        Task<IEnumerable<POLineItemArticleMasterViewModel>> GetInformationFromArticleMaster(string customerOrgCode);

        Task<ReportingMetricPOViewModel> GetReportingPOs(bool isInternal, string affiliates, string statisticFilter);

        Task DeleteAsync(string poKey);
        Task<IEnumerable<PrincipalDropdownListItemViewModel>> GetPrincipalDataSourceForMultiPOsSelectionAsync(bool isInternal, long roleId, long organizationId, string affiliates);

        #region PO Statistics on dashboard

        Task<StatisticsPOViewModel> GetBookedPOStatistics(bool isInternal, string affiliates, string statisticFilter);
        Task<StatisticsPOViewModel> GetUnbookedPOStatistics(bool isInternal, string affiliates, string statisticFilter);
        Task<StatisticsPOManagedToDateViewModel> GetManagedToDatePOStatistics(bool isInternal, string affiliates, string statisticFilter);
        Task<StatisticsPOInOriginDCViewModel> GetInOriginDCPOStatistics(bool isInternal, string affiliates, string statisticFilter);
        Task<StatisticsPOInTransitViewModel> GetInTransitPOStatistics(bool isInternal, string affiliates, string statisticFilter);
        Task<StatisticsPOCustomsClearedViewModel> GetCustomsClearedPOStatistics(bool isInternal, string affiliates, string statisticFilter);
        Task<StatisticsPOPendingDCDeliveryViewModel> GetPendingDCDeliveryPOStatistics(bool isInternal, string affiliates, string statisticFilter);
        Task<StatisticsPODCDeliveryConfirmedViewModel> GetDCDeliveryConfirmedPOStatistics(bool isInternal, string affiliates, string statisticFilter);

        Task<ListPagingViewModel<string>> SearchCategorizedPOAsync(CategorizedPOType type, string searchTearm, int page, int pageSize, bool isInternal, string affiliates, string supplierCustomerRelationships, long? delegatedOrganizationId = 0);

        #endregion PO Statistics on dashboard

        Task<IEnumerable<POEmailNotificationQueryModel>> GetPOEmailNotificationAsync(long buyerComplianceId);
        IEnumerable<POEmailNotificationViewModel> CreatePOEmailNotificationAsync(long buyerComplianceId, IEnumerable<POEmailNotificationQueryModel> pos);

        Task ChangeStageToShipmentDispatchAsync(IEnumerable<long> poIds);
        Task ChangeStageToCloseAsync(IEnumerable<long> poIds, string userName, DateTime eventDate, string location = null, string remark = null);
        Task RevertStageToReleasedAsync(IEnumerable<long> poIds);
        Task RevertStageToShipmentDispatchAsync(IEnumerable<long> poIds);
        Task<IEnumerable<CargoDetailModel>> GetCargoDetails(List<long> poIds);
        void AdjustQuantityOnPOLineItems(IEnumerable<long> shipmentIds, IEnumerable<long> poIds, AdjustBalanceOnPOLineItemsType type);

        Task ChangeStagePOWithoutBookingAsync(long consignmentId);
    }
}