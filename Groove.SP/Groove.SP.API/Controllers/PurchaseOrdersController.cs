using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using System.Collections.Generic;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Groove.SP.Application.ImportData.Services.Interfaces;
using Groove.SP.Application.Providers.BlobStorage;
using Microsoft.Extensions.Options;
using Groove.SP.Application.Activity.Services.Interfaces;
using System.Linq;
using System;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Note.Services.Interfaces;
using Hangfire;
using Groove.SP.API.Filters;
using Groove.SP.Application.GlobalIdMasterDialog.Services.Interfaces;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Groove.SP.Core.Entities;
using AutoMapper;
using Groove.SP.Core.Data;
using System.Threading;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Application.ViewSetting.Services.Interfaces;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPurchaseOrderListService _purchaseOrderListService;
        private readonly ITranslationProvider _translation;
        private readonly IBlobStorage _blobStorage;
        private readonly AppConfig _appConfig;
        private readonly IImportDataProgressService _importDataProgressService;
        private readonly IActivityService _activityService;
        private readonly IGlobalIdMasterDialogService _globalIdMasterDialogService;
        private readonly IViewSettingService _viewSettingService;
        private readonly INoteService _noteService;
        private readonly IMapper _mapper;

        public PurchaseOrdersController(
            IPurchaseOrderService purchaseOrderService,
            ITranslationProvider translation,
            IBlobStorage blobStorage,
            IOptions<AppConfig> appConfig,
            IImportDataProgressService importDataProgressService,
            IActivityService activityService,
            IPurchaseOrderListService purchaseOrderListService,
            INoteService noteService,
            IMapper mapper,
            IGlobalIdMasterDialogService globalIdMasterDialogService,
            IViewSettingService viewSettingService)
        {
            _purchaseOrderService = purchaseOrderService;
            _appConfig = appConfig.Value;
            _translation = translation;
            _blobStorage = blobStorage;
            _importDataProgressService = importDataProgressService;
            _activityService = activityService;
            _purchaseOrderListService = purchaseOrderListService;
            _noteService = noteService;
            _mapper = mapper;
            _globalIdMasterDialogService = globalIdMasterDialogService;
            _viewSettingService = viewSettingService;
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.PO_List)]
        public async Task<IActionResult> Get([DataSourceRequest] DataSourceRequest request, string affiliates, string customerRelationships = "", bool isExport = false, string statisticKey = "", long? organizationId = 0, string statisticFilter = "", string statisticValue = "", long? id = null)
        {
            var viewModels = await _purchaseOrderListService.ListAsync(request, CurrentUser.IsInternal, affiliates, customerRelationships, organizationId, id, statisticKey, statisticFilter, statisticValue, null, isExport);
            if (isExport)
            {
                viewModels.Data = _mapper.Map<IEnumerable<PurchaseOrderListViewModel>>(viewModels.Data);
                foreach (var po in viewModels.Data as IEnumerable<PurchaseOrderListViewModel>)
                {
                    po.StatusName = await _translation.GetTranslationByKeyAsync(po.StatusName);
                    po.StageName = await _translation.GetTranslationByKeyAsync(po.StageName);
                    po.ContainerTypeName = await _translation.GetTranslationByKeyAsync(EnumHelper<EquipmentType>.GetDisplayName(po.ContainerType));
                }
            }
            else
            {
                var data = viewModels.Data as IList<PurchaseOrderQueryModel>;
                foreach (var po in data)
                {
                    po.CustomerReferences = null;
                    po.ModeOfTransport = null;
                    po.ShipFrom = null;
                    po.ShipTo = null;
                    po.ExpectedDeliveryDate = null;
                    po.ExpectedShipDate = null;
                    po.PORemark = null;
                }

                // Toggle field response by current user role
                var vwSettings = await _viewSettingService.ApplyViewSettingsAsync(viewModels.Data, request.ViewSettingModuleId, CurrentUser.UserRoleId);

                // Respond back to the client for GUI rendering purposes.
                viewModels.ViewSettings = vwSettings;
            }

            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("search/{itemNo}")]
        [AppAuthorize(AppPermissions.PO_List)]
        public async Task<IActionResult> GetListFromQuickSearchAsync([DataSourceRequest] DataSourceRequest request, string affiliates, string customerRelationships = "", bool isExport = false, string statisticKey = "", long? organizationId = 0, string statisticFilter = "", string statisticValue = "", string itemNo = "", long? id = null)
        {
            var viewModels = await _purchaseOrderListService.ListAsync(request, CurrentUser.IsInternal, affiliates, customerRelationships, organizationId,id, statisticKey, statisticFilter, statisticValue, itemNo, isExport);
            if (isExport)
            {
                viewModels.Data = _mapper.Map<IEnumerable<PurchaseOrderListViewModel>>(viewModels.Data);
                foreach (var po in viewModels.Data as IEnumerable<PurchaseOrderListViewModel>)
                {
                    po.StatusName = await _translation.GetTranslationByKeyAsync(po.StatusName);
                    po.StageName = await _translation.GetTranslationByKeyAsync(po.StageName);
                    po.ContainerTypeName = await _translation.GetTranslationByKeyAsync(EnumHelper<EquipmentType>.GetDisplayName(po.ContainerType));
                }
            }
            else
            {
                var data = viewModels.Data as IList<PurchaseOrderQueryModel>;
                foreach (var po in data)
                {
                    po.CustomerReferences = null;
                    po.ModeOfTransport = null;
                    po.ShipFrom = null;
                    po.ShipTo = null;
                    po.Incoterm = null;
                    po.ExpectedDeliveryDate = null;
                    po.ExpectedShipDate = null;
                    po.PORemark = null;
                }

                // Toggle field response by current user role
                var vwSettings = await _viewSettingService.ApplyViewSettingsAsync(viewModels.Data, request.ViewSettingModuleId, CurrentUser.UserRoleId);

                // Respond back to the client for GUI rendering purposes.
                viewModels.ViewSettings = vwSettings;
            }
            return new JsonResult(viewModels);
        }

        /// <summary>
        /// To get full information of purchase order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="affiliates"></param>
        /// <param name="customerRelationships"></param>
        /// <param name="organizationId"></param>
        /// <param name="replacedByOrganizationReferences">Whether HS code, Chinese description will be replaced from Organization preferences</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> Get(long id, string affiliates, string customerRelationships, long? organizationId = 0, bool? replacedByOrganizationReferences = false)
        {
            
            var viewModels = await _purchaseOrderService.GetAsync(id, CurrentUser, affiliates, customerRelationships, organizationId, replacedByOrganizationReferences);

            _viewSettingService.SetViewSettingModuleId(ViewSettingModuleId.PO_DETAIL_BOOKINGS, viewModels.Shipments);

            // Toggle field response by current user role
            await _viewSettingService.ApplyViewSettingsAsync(viewModels, CurrentUser.UserRoleId);

            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("purchaseOrderSelections/{principalId}")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> GetPurchaseOrderSelectionsBySearching(long principalId, int skip, int take, long organizationId, string searchTerm, long selectedPOId, int roleId, string affiliates, string customerRelationships = "")
        {
            var viewModels = await _purchaseOrderListService.GetPurchaseOrderSelectionsByPrincipalIdAsync(skip, take, searchTerm, selectedPOId, customerRelationships, principalId, organizationId, roleId, affiliates, CurrentUser.IsInternal);

            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("unmappedPurchaseOrderSelections")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> GetUnmappedPurchaseOrderSelectionsBySearching(long principalId, long supplierId, int skip, int take, string searchTerm)
        {
            var viewModels = await _purchaseOrderListService.GetUnmappedPurchaseOrderSelectionsAsync(skip, take, searchTerm, principalId, supplierId);

            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("unmappedPurchaseOrderTotalCount")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<long> GetUnmappedPurchaseOrderTotalCountAsync(long principalId, long supplierId)
        {
            long recordCount = 0;

            var viewModels = await _purchaseOrderListService.GetUnmappedPurchaseOrderSelectionsAsync(0, 1, string.Empty, principalId, supplierId);
            if (viewModels is not null)
            {
                recordCount = viewModels.FirstOrDefault()?.RecordCount ?? 0;
            }

            return recordCount;
        }

        [HttpPost]
        [Route("assignPurchaseOrders")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> AssignPurchaseOrdersAsync([FromBody] AssignPurchaseOrdersViewModel model)
        {
            await _purchaseOrderService.AssignPOsAsync(model, CurrentUser.Username);

            return new JsonResult(null);
        }

        /// <summary>
        /// To get data for customer PO data source in POFF page
        /// </summary>
        /// <param name="poIds">String id list of purchase orders' ids</param>
        /// <param name="customerOrgCode">Customer organization code</param>
        /// <param name="replacedByOrganizationReferences">Whether HS code, Chinese description will be replaced from Organization preferences</param>
        /// <returns></returns>
        [HttpGet]
        [Route("customerPurchaseOrders/list")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> GetCustomerPOListByIds(string poIds, string customerOrgCode, bool replacedByOrganizationReferences)
        {
            // Do not replace HS code, Chinese description if internal users
            if (CurrentUser.IsInternal)
            {
                replacedByOrganizationReferences = false;
            }
            var viewModels = await _purchaseOrderService.GetCustomerPOListByIds(poIds, customerOrgCode, replacedByOrganizationReferences, CurrentUser.OrganizationId);
            await _purchaseOrderService.UpdateContactsToLatestByOrgCodesAsync(viewModels.SelectMany(c => c.Contacts));
            return new JsonResult(viewModels);
        }

        /// <summary>
        /// To fetch data for customer PO data source in POFF page, applying searching on server side
        /// Which is used to add/edit/remove PO line item on tab Customer PO
        /// </summary>
        /// <param name="replacedByOrganizationReferences">Whether HS code, Chinese description will be replaced from Organization preferences</param>
        /// <returns></returns>
        [HttpGet]
        [Route("customerPurchaseOrders/search")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> GetCustomerPOListBySearching(long customerOrgId, string customerOrgCode, long supplierOrgId, string supplierCompanyName, string searchType, string searchTerm, string affiliates, POType selectedPOType, long selectedPOId, int skip, int take, bool replacedByOrganizationReferences)
        {
            if (customerOrgId == 0 || string.IsNullOrEmpty(customerOrgCode) || string.IsNullOrEmpty(searchType) || take == 0)
            {
                throw new ApplicationException("Filter set is not correct.");
            }
            var viewModels = await _purchaseOrderService.GetCustomerPOListBySearching(skip, take, searchType, searchTerm, selectedPOId, selectedPOType, affiliates, customerOrgId, customerOrgCode, supplierOrgId, supplierCompanyName, CurrentUser, replacedByOrganizationReferences);
            await _purchaseOrderService.UpdateContactsToLatestByOrgCodesAsync(viewModels.SelectMany(c => c.Contacts));
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("progressCheck/search")]
        [AppAuthorize(AppPermissions.PO_ProgressCheckCRD)]
        public async Task<IActionResult> SearchPOsForProgressCheckAsync(string affiliates)
        {
            var queryParams = HttpContext.Request.Query;
            var filter = new Dictionary<string, string>();

            foreach (var key in queryParams.Keys)
            {
                StringValues @value = new StringValues();
                queryParams.TryGetValue(key, out @value);
                filter.Add(key, @value.ToString());
            }

            var jsonFilter = JsonConvert.SerializeObject(filter);
            var result = await _purchaseOrderService.SearchPOsForProgressCheckAsync(jsonFilter, CurrentUser, affiliates);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("progressCheck/searchFromEmail")]
        [AppAuthorize(AppPermissions.PO_ProgressCheckCRD)]
        public async Task<IActionResult> SearchPOsForProgressCheckFromEmailAsync([FromQuery] int buyerComplianceId, [FromQuery] string poIds)
        {
            var result = await _purchaseOrderService.SearchPOsForProgressCheckFromEmailAsync(buyerComplianceId, poIds);
            return new JsonResult(result);
        }

        [HttpPut("progressCheck")]
        [AppAuthorize(AppPermissions.PO_ProgressCheckCRD)]
        public async Task<IActionResult> UpdatePOProgressCheckRangeAsync([FromBody] IEnumerable<PurchaseOrderProgressCheckViewModel> model)
        {
            var result = await _purchaseOrderService.UpdatePOProgressCheckRangeAsync(model, CurrentUser);
            return Ok(result);
        }

        #region Activities
        [HttpGet]
        [Route("{id}/activities")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> GetActivitiesByPO(long id)
        {
            var result = await _activityService.GetActivities(EntityType.CustomerPO, id);
            return new JsonResult(result);
        }

        [HttpPost("{id}/activities")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> PostActivityAsync(long id, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> PutActivityAsync(long id, long activityId, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.UpdateAsync(model, activityId, CurrentUser);
            return Ok();
        }

        [HttpDelete("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> DeleteActivityAsync(long activityId)
        {
            await _activityService.DeleteAsync(activityId);
            return Ok();
        }
        #endregion

        [HttpGet]
        [Route("{id}/POLineItems/{poLineItemId}")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetPOLineItem(long id, long poLineItemId)
        {
            var result = await _purchaseOrderService.GetPOLineItem(id, poLineItemId);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/POLineItems/{productCode}/ArticleMaster")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> GetPOLineItemArticleMaster(long id, string productCode)
        {
            var result = await _purchaseOrderService.GetInformationFromArticleMaster(id, productCode);
            return new JsonResult(result);
        }

        [HttpGet("{id}/notes")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> GetNotesByPO(long id)
        {
            var result = await _noteService.GetPurchaseOrderNotesByIdAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/MasterDialogs")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> GetMasterDialogsByPO(long id)
        {
            var result = await _globalIdMasterDialogService.GetByPurchaseOrderAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [AppAuthorize(AppPermissions.PO_List)]
        [Route("DownloadExcelTemplate")]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _blobStorage.GetBlobAsByteArrayAsync(_appConfig.BlobStorage.PurchaseOrderTemplate);

            Response.Headers.Add("content-disposition", $"attachment; filename=PO_Import.xlsx");
            return File(content, "application/octet-stream");
        }
        [HttpPost]
        [Route("import")]
        [AppAuthorize(AppPermissions.PO_List)]
        public async Task<IActionResult> ImportFromExcel([FromForm] IFormFile files)
        {
            if (files == null || files.Length < 0)
            {
                return BadRequest();
            }

            string userName = CurrentUser.Username;
            var importDataProgress = await _importDataProgressService.CreateAsync("Import Purchase Order From Excel", userName);
            BackgroundJob.Enqueue<IPurchaseOrderService>(s => s.ImportFromExcel(files.GetAllBytes(), userName, importDataProgress.Id));

            return Ok(importDataProgress.Id);
        }

        [HttpPost("lists")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> PostAsync([FromBody] IEnumerable<CreatePOViewModel> model)
        {
            var result = await _purchaseOrderService.CreateAsync(model, CurrentUser.Username);
            return Ok(result);
        }

        /// <summary>
        /// It is to fetch data source for list of Principal in popup to select multiple POs to create booking.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="roleId"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        [HttpGet]
        [AppAuthorize()]
        [Route("PrincipalsByPOs")]
        public async Task<IActionResult> GetPrincipalDataSourceForMultiPOsSelectionAsync(long organizationId, long roleId, string affiliates)
        {
            var viewModels = await _purchaseOrderService.GetPrincipalDataSourceForMultiPOsSelectionAsync(CurrentUser.IsInternal, roleId, organizationId, affiliates);

            return new JsonResult(viewModels);
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> PostAsync([FromBody] CreatePOViewModel model)
        {
            var result = await _purchaseOrderService.CreateAsync(model, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut("{poKey}")]
        [AppAuthorize(AppPermissions.PO_Detail_Edit)]
        public async Task<IActionResult> PutAsync(string poKey, [FromBody] UpdatePOViewModel model)
        {
            var result = await _purchaseOrderService.UpdateAsync(poKey, model);
            return Ok(result);
        }

        [HttpPut("{id}/delegate")]
        [AppAuthorize(AppPermissions.PO_Detail_Edit, AppPermissions.PO_Delegation)]
        public async Task<IActionResult> DelegateAsync(long id, [FromBody] DelegationPOViewModel model)
        {
            model.UpdatedBy = CurrentUser.Username;

            await _purchaseOrderService.DelegateAsync(id, model);
            return Ok();
        }

        [HttpPut("{id}/close")]
        [AppAuthorize(AppPermissions.PO_Detail_Close)]
        public async Task<IActionResult> CloseAsync(long id, [FromBody] UpdatePOViewModel viewModel)
        {
            await _purchaseOrderService.CloseAsync(id, viewModel, CurrentUser.Username);
            return Ok();
        }

        [HttpDelete("{poKey}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> DeleteAsync(string poKey)
        {
            await _purchaseOrderService.DeleteAsync(poKey);
            return Ok();
        }



        #region Shipped purchase orders

        /// <summary>
        /// To fetch data for Shipper purchase orders grid
        /// </summary>
        /// <param name="request"></param>
        /// <param name="affiliates"></param>
        /// <param name="customerRelationships"></param>
        /// <param name="isExport"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("shipped/search")]
        [AppAuthorize(AppPermissions.PO_List)]
        public async Task<IActionResult> ShippedGet([DataSourceRequest] DataSourceRequest request, string affiliates, string customerRelationships = "", bool isExport = false, long? organizationId = 0)
        {
            var viewModels = await _purchaseOrderListService.ShippedListAsync(request, CurrentUser.IsInternal, affiliates, customerRelationships, organizationId);

            if (isExport)
            {
                var data = viewModels.Data as IList<PurchaseOrderListViewModel>;
                foreach (var po in data)
                {
                    po.StatusName = await _translation.GetTranslationByKeyAsync(EnumHelper<PurchaseOrderStatus>.GetDisplayName(po.Status));
                    po.StageName = await _translation.GetTranslationByKeyAsync(EnumHelper<POStageType>.GetDisplayName(po.Stage));
                }
            }

            return new JsonResult(viewModels);
        }

        #endregion Shipped purchase orders

        #region PO Statistics on dashboard

        [HttpGet]
        [Route("reporting")]
        [AppAuthorize(AppPermissions.Dashboard_ThisWeekCustomerPO)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetReportingPOs(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetReportingPOs(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("statistics/booked")]
        [AppAuthorize(AppPermissions.Dashboard_EndToEndShipmentStatus)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetBookedPOStatistics(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetBookedPOStatistics(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("statistics/unbooked")]
        [AppAuthorize(AppPermissions.Dashboard_EndToEndShipmentStatus)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetUnbookedPOStatistics(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetUnbookedPOStatistics(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("statistics/managed-to-date")]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetManagedToDatePOStatistics(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetManagedToDatePOStatistics(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [Route("statistics/in-origin-dc")]
        [AppAuthorize(AppPermissions.Dashboard_EndToEndShipmentStatus)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetInOriginDCPOStatistics(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetInOriginDCPOStatistics(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("statistics/in-transit")]
        [AppAuthorize(AppPermissions.Dashboard_EndToEndShipmentStatus)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetInTransitPOStatistics(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetInTransitPOStatistics(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("statistics/customs-cleared")]
        [AppAuthorize(AppPermissions.Dashboard_EndToEndShipmentStatus)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetCustomsClearedPOStatistics(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetCustomsClearedPOStatistics(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("statistics/pending-dc-delivery")]
        [AppAuthorize(AppPermissions.Dashboard_EndToEndShipmentStatus)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetPendingDCDeliveryPOStatistics(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetPendingDCDeliveryPOStatistics(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("statistics/dc-delivery-confirmed")]
        [AppAuthorize(AppPermissions.Dashboard_EndToEndShipmentStatus)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetDCDeliveryConfirmedPOStatistics(string affiliates, string statisticFilter)
        {
            var result = await _purchaseOrderService.GetDCDeliveryConfirmedPOStatistics(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("search-categorized-po")]
        [AppAuthorize(AppPermissions.Dashboard_CategorizedPO)]
        [AppDataMemoryCache]
        public async Task<IActionResult> SearchCategorizedPOAsync(CategorizedPOType type, string searchTerm, int pageOffset, int pageSize, string affiliates, string customerRelationships = "", long? organizationId = 0)
        {
            var result = await _purchaseOrderService.SearchCategorizedPOAsync(type, searchTerm, pageOffset, pageSize, CurrentUser.IsInternal, affiliates, customerRelationships, organizationId);
            return new JsonResult(result);
        }

        #endregion


    }
}
