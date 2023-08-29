using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.ImportData.Services;
using Groove.SP.Application.ImportData.Services.Interfaces;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Application.Translations.Helpers;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class ImportDataProgressController : ControllerBase
    {
        private readonly IImportDataProgressService _importDataProgressService;
        private readonly IBlobStorage _blobStorage;

        public ImportDataProgressController(IImportDataProgressService importDataProgressService, IBlobStorage blobStorage)
        {
            _importDataProgressService = importDataProgressService;
            _blobStorage = blobStorage;
        }


        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var model = await _importDataProgressService.GetByIdAsync(id);
            return Ok(model);
        }

        /// <summary>
        /// To get template file to import data
        /// </summary>
        /// <param name="type">Template type:
        /// <list type="number">
        /// <item>orgnization: to import organizations</item>
        /// <item>user: to import external users</item>
        /// </list>
        /// </param>
        /// <returns></returns>
        [Route("template/{type:required}")]
        public async Task<IActionResult> GetDataImportTemplateAsync(string type)
        {
            byte[] content;
            switch (type.ToLowerInvariant())
            {
                case "organization":
                    content = await _blobStorage.GetBlobByRelativePathAsync($"template:organization:OrganizationImportTemplate.xlsx");
                    Response.Headers.Add("content-disposition", $"attachment; filename=OrganizationImportTemplate.xlsx");
                    return File(content, "application/octet-stream");
                case "user":
                    content = await _blobStorage.GetBlobByRelativePathAsync($"template:user:UserImportTemplate.xlsx");
                    Response.Headers.Add("content-disposition", $"attachment; filename=UserImportTemplate.xlsx");
                    return File(content, "application/octet-stream");
                default:
                    return null;
            }
            
        }

        [HttpPost]
        [Route("organizations")]
        [AppAuthorize(AppPermissions.Organization_Detail_Add)]
        public async Task<IActionResult> ImportOrganizationAsync(IFormFile files)
        {
            if (files == null || files.Length <= 0)
            {
                return BadRequest();
            }

            string userName = CurrentUser.Username;
            var importDataProgress = await _importDataProgressService.CreateAsync("Import Organizations from Excel", userName);
            BackgroundJob.Enqueue<ImportDataProgressService>(s => s.ImportOrganizationsFromExcelSilentAsync(importDataProgress.Id, userName, files.FileName, files.GetAllBytes()));

            return Ok(importDataProgress.Id);
        }

        [HttpGet]
        [Route("{id}/downloadErrors")]
        public async Task<IActionResult> DownloadErrorAsync(long id, [FromQuery] string lang = TranslationUtility.ENGLISH_KEY)
        {
            var bytes = await _importDataProgressService.ExportErrorExcelAsync(id, CurrentUser.Name, lang);

            if (bytes == null)
            {
                return NoContent();
            }

            return File(bytes, MimeTypes.TextXlsx, $"ErrorList.xlsx");
        }

        [HttpGet]
        [Route("{id}/downloadPOErrors")]
        public async Task<IActionResult> DownloadPOErrorAsync(long id, [FromQuery] string lang = TranslationUtility.ENGLISH_KEY)
        {
            var bytes = await _importDataProgressService.ExportPOErrorExcelAsync(id, CurrentUser.Name, lang);

            if (bytes == null)
            {
                return NoContent();
            }

            return File(bytes, MimeTypes.TextXlsx, $"ErrorList.xlsx");
        }
    }
}