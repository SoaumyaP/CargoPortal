using Groove.SP.Application.Authorization;
using Groove.SP.Application.Reports.Services.Interfaces;
using Groove.SP.Application.Reports.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using Groove.SP.Core.Models;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportListService _reportListService;
        private readonly IReportService _reportService;

        public ReportsController(
            IReportListService reportListService,
            IReportService reportService)
        {
            _reportListService = reportListService;
            _reportService = reportService;
        }


        /// <summary>
        /// To fetch data for report list
        /// </summary>
        /// <param name="request">Meta data of requesting from grid</param>
        /// <param name="roleId">Role of current user</param>
        /// <param name="selectedOrganizationId">Id of selected principal</param>
        /// <param name="organizationIds">String of accessible organization ids by current user</param>
        /// <param name="affiliates">Affiliates string of external user. Ex: [2,127,128]</param>
        /// <returns></returns>
        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Reports_List)]
        public async Task<IActionResult> Get([DataSourceRequest] DataSourceRequest request, long roleId, long selectedOrganizationId, string organizationIds, string affiliates)
        {
            var viewModels = await _reportListService.ListAsync(request, CurrentUser.IsInternal, roleId, selectedOrganizationId, affiliates);

            return new JsonResult(viewModels);
        }

        [HttpPut]
        [Route("{id}/permissions")]
        [AppAuthorize(AppPermissions.Reports_List)]
        public IActionResult GrantReportPermission(ReportGrantPermissionViewModel data)
        {
            if (!CurrentUser.IsInternal)
            {
                return Forbid();
            }

            if (data == null || data.ReportId == 0)
            {
                return BadRequest();
            }

            data.CreatedBy = CurrentUser.Email;
            data.CreatedDate = DateTime.UtcNow;

            var result = _reportService.GrantReportPermission(data);

            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/permissions")]
        [AppAuthorize(AppPermissions.Reports_List)]
        public async Task<IActionResult> GetReportPermissionAsync(long id)
        {
            if (!CurrentUser.IsInternal)
            {
                return Forbid();
            }

            if (id == 0)
            {
                return BadRequest();
            }
         
            var result = await _reportService.GetReportPermissionAsync(id);

            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/authorized")]
        [AppAuthorize]
        public async Task<IActionResult> GetReportAsync(long id, long selectedOrganizationId)
        {
            var isAuthorized = await _reportService.IsAuthorized(id, selectedOrganizationId, CurrentUser);

            if (!isAuthorized)
            {
                return Forbid();
            }         

            return Ok();
        }

        [HttpGet("{id}/export")]
        [AppAuthorize]
        public async Task<IActionResult> ExportXlsx(long id, long selectedCustomerId)
        {
            var isAuthorized = await _reportService.IsAuthorized(id, selectedCustomerId, CurrentUser);

            if (!isAuthorized)
            {
                return Forbid();
            }

            var queryParams = HttpContext.Request.Query;
            var filter = new Dictionary<string, string>();

            foreach (var key in queryParams.Keys)
            {
                StringValues @value = new StringValues();
                queryParams.TryGetValue(key, out @value);
                filter.Add(key, @value.ToString());
            }

            if (CurrentUser.UserRoleId == (int)Role.Shipper)
            {
                filter.Add("SupplierId", CurrentUser.OrganizationId.ToString());
            }
            
            var jsonFilter = JsonConvert.SerializeObject(filter);
            var report = await _reportService.GetAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            var bytes = await _reportService.ExportDataAsync(report.Id, jsonFilter);

            if (bytes == null)
            {
                return NoContent();
            }

            return File(bytes, MimeTypes.TextXlsx, $"{report.ReportName}.xlsx"); 
        }

        [HttpGet("{id}/token")]
        [AppAuthorize]
        public async Task<IActionResult> GetReportAccessToken(long id, long selectedCustomerId)
        {
            // Check if current user can access a report
            var isAuthorized = await _reportService.IsAuthorized(id, selectedCustomerId, CurrentUser);
            if (!isAuthorized)
            {
                return Forbid();
            }

            // Get report token
            var accessToken = await _reportService.GetTelerikAccessToken(CurrentUser);
            return new JsonResult(accessToken);
        }

        [HttpGet("selectOptions")]
        [AppAuthorize]
        public async Task<IActionResult> GetReportOptions(bool isInternal, long roleId, string affiliates)
        {
            var viewModels = await _reportListService.SchedulingReportOptionsAsync(isInternal, roleId, affiliates);

            return new JsonResult(viewModels);
        }

        /// <summary>
        /// To get Telerik access token
        /// </summary>
        /// <returns></returns>
        [HttpGet("telerikAccessToken")]
        [AppAuthorize]
        public async Task<IActionResult> GetTelerikAccessToken()
        {
            // Get report token
            var accessToken = await _reportService.GetTelerikAccessToken(CurrentUser);
            return new JsonResult(accessToken);
        }

    }
    
}
