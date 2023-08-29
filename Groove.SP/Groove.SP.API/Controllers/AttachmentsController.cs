using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.AppDocument.Services.Interfaces;
using Groove.SP.Application.Provider.EmailSender;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RazorLight;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Authorization;
using Groove.SP.Infrastructure.EmailSender.Models;
using Groove.SP.Infrastructure.BlobStorage;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController : ControllerBase
    {
        private readonly AppConfig _appConfig;
        private readonly IRazorLightEngine _razorLightEngine;
        private readonly IEmailSender _emailSender;
        private readonly IBlobStorage _blobStorage;
        private readonly IShareDocumentService _shareDocumentService;
        private readonly IAttachmentService _attachmentService;
        private readonly IUserProfileService _userProfileService;

        public AttachmentsController(IOptions<AppConfig> appConfig, 
            IRazorLightEngine razorLightEngine,
            IEmailSender emailSender, 
            IBlobStorage blobStorage,
            IShareDocumentService appDocumentService,
            IAttachmentService attachmentService,
            IUserProfileService userProfileService)
        {
            _appConfig = appConfig.Value;
            _razorLightEngine = razorLightEngine;
            _emailSender = emailSender;
            _blobStorage = blobStorage;
            _shareDocumentService = appDocumentService;
            _attachmentService = attachmentService;
            _userProfileService = userProfileService;
        }
        
        /// <summary>
        /// Called from application GUI to upload attachment (as clicking Add button on Upload file pop-up)
        /// <br></br><b>Not for Booking/POFulfillment page</b>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync(AttachmentViewModel model)
        {
            var result = await _attachmentService.ImportAttachmentAsync(model, true, CurrentUser.UserRoleId);
            return Ok(result);
        }

        /// <summary>
        /// Called from application GUI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(long id, AttachmentViewModel model)
        {
            var result = await _attachmentService.UpdateAttachmentAsync(model, CurrentUser.UserRoleId);
            return Ok(result);
        }

        /// <summary>
        /// To delete attachment, called from application GUI <em>(Applied attachment permissions)</em>
        /// <br></br><em>Remove attachment record after all links (GlobalIdAttachments) removed </em>
        /// </summary>
        /// <param name="globalId">Global id</param>
        /// <param name="attachmentId">Attachment id</param>
        /// <returns></returns>
        [HttpDelete()]
        public async Task<IActionResult> DeleteAttachmentAsync(string globalId, long attachmentId)
        {
            var result = await _attachmentService.DeleteAttachmentAsync(globalId, attachmentId, CurrentUser.UserRoleId);
            return Ok(result);
        }

        [HttpPost]
        [Route("Share")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail, 
            AppPermissions.BillOfLading_MasterBLDetail,
            AppPermissions.BillOfLading_HouseBLDetail, 
            AppPermissions.Shipment_Detail,
            AppPermissions.PO_Fulfillment_Detail)
        ]
        public async Task<IActionResult> ShareFiles([FromBody]ShareAttachmentRequestModel shareRequest)
        {
            if (shareRequest != null && shareRequest.MailingList.Any() && shareRequest.SelectedAttachments.Any())
            {
                var shareDocument = await CreateCompressedShareFile(shareRequest.SelectedAttachments);
                var tempFileName = Path.GetTempPath() + shareDocument.FileName;
                var tempFile = await _blobStorage.GetBlobAsByteArrayAsync(shareDocument.BlobId);
                var userProfile = await _userProfileService.GetAsync(CurrentUser.Username);

                var shareFileEmailParameters = new ShareFileEmailParameters
                {
                    CustomerName = userProfile.Name,
                    ShareFileExpiredTime = _appConfig.Email.AttachmentExpiredTime.ToString(),
                    ShareFileLink = $"http://{HttpContext.Request.Host.Value}/api/Attachments/DownloadShareFile/?docId={shareDocument.Id}&fileName={shareDocument.FileName}",
                    FileName = shareDocument.FileName,
                    SupportEmail =_appConfig.SupportEmail
                };
                System.IO.File.WriteAllBytes(tempFileName, tempFile);

                string emailBody = await _razorLightEngine.CompileRenderAsync("EmailShareFiles", shareFileEmailParameters);
                _emailSender.SendEmail(shareRequest.MailingList, $" Shipment Portal: {userProfile.Name} has shared some documents with you", emailBody);
            }

            return Ok();
        }

        [HttpPost]
        [Route("Download")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail,
            AppPermissions.Shipment_ContainerDetail,
            AppPermissions.BillOfLading_MasterBLDetail,
            AppPermissions.BillOfLading_HouseBLDetail,
            AppPermissions.Shipment_Detail)
        ]
        public async Task<IActionResult> DownloadZipFilesAsync([FromBody] IEnumerable<AttachmentModel> attachments)
        {
            if (attachments.Any())
            {
                var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var document in attachments)
                    {
                        var entryFile = archive.CreateEntry(document.FileName, CompressionLevel.NoCompression);
                        var docFile = await _blobStorage.GetBlobAsByteArrayAsync(document.BlobId);

                        using (var entryContent = entryFile.Open())
                        {
                            using (var binWriter = new BinaryWriter(entryContent))
                            {
                                binWriter.Write(docFile);
                            }
                        }
                    }
                }
                zipStream.Position = 0;
                return File(zipStream, "application/octet-stream");
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadShareFile")]
        public async Task<IActionResult> DownloadShareFile(long docId, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var document = await _shareDocumentService.GetAsync(docId);
                int.TryParse(_appConfig.Email.AttachmentExpiredTime.ToString(), out var expiredTime);
                if (document != null && fileName.Equals(document.FileName)
                        && document.UpdatedDate.HasValue && DateTime.UtcNow.Subtract(document.UpdatedDate.Value).TotalHours < expiredTime)
                {
                    return await GetDownloadFile(document.FileName, document.BlobId);
                }
            }
            
            return Redirect(_appConfig.ClientUrl + "/error/expired");
        }

        private async Task<FileContentResult> GetDownloadFile(string fileName, string blobPointer)
        {
            byte[] content = await _blobStorage.GetBlobAsByteArrayAsync(blobPointer);
            return File(content, "application/octet-stream", fileName);
        }
        
        [HttpGet]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail, 
            AppPermissions.BillOfLading_MasterBLDetail,
            AppPermissions.BillOfLading_HouseBLDetail, 
            AppPermissions.Shipment_Detail,
            AppPermissions.PO_Fulfillment_Detail)
        ]
        [Route("{id}/download/{fileName}")]
        public async Task<IActionResult> DownloadAttachment(long id, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var document = await _attachmentService.GetAsync(id);
                if (document != null && fileName.Equals(document.FileName))
                {
                    return await GetDownloadFile(document.FileName, document.BlobId);
                }
            }
            return null;
        }

        /// <summary>
        /// Called from third-party via public API
        /// <br></br><b>No need to verify on attachment permissions by attachment type</b>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        public async Task<IActionResult> ImportAttachment([FromForm] ImportAttachmentViewModel model)
        {
            var stream = model.File.OpenReadStream();
            var fileName = model.File.FileName;
            var blobId = await _blobStorage.PutBlobAsync(BlobCategories.ATTACHMENT, fileName, stream);

            var viewModel = new AttachmentViewModel()
            {
                Id = model.Id,
                AttachmentType = model.AttachmentType,
                ReferenceNo = model.ReferenceNo,
                Description = model.Description,
                UploadedBy = model.UploadedBy,
                UploadedDateTime = model.UploadedDate,

                BillOfLadingId = model.BillOfLadingId,
                ContainerId = model.ContainerId,
                MasterBillOfLadingId = model.MasterBillId,
                ShipmentId = model.ShipmentId,
                POFulfillmentId = model.POFulfillmentId,
                BlobId = blobId,
                FileName = fileName
            };
            var attachmentId = await _attachmentService.ImportAttachmentAsync(viewModel);

            var attachment = new {id = attachmentId, fileName = fileName};
            var downloadUrl = Url.Action("DownloadAttachment", attachment);

            return Created(downloadUrl, attachment);
        }
        
        /// <summary>
        /// Called from application GUI
        /// <br></br><b>As user select file to update on Upload file pop-up</b>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ImportFile")]
        public async Task<IActionResult> ImportAttachment([FromForm] ImportFileViewModel model)
        {
            var stream = model.Files.OpenReadStream();
            var fileName = model.Files.FileName;
            var blobId = await _blobStorage.PutBlobAsync(BlobCategories.ATTACHMENT, fileName, stream);
            return new JsonResult(new { blobId, fileName });
        }

        /// <summary>
        /// Called from application GUI to get accessible document types by current user role and entity type
        /// </summary>
        /// <param name="roleId">Role id. See more values at <see cref="Groove.SP.Core.Models.Role"/></param>
        /// <param name="entityType">Entity type. See more values at <see cref="Groove.SP.Core.Models.EntityType"/></param>
        /// <returns></returns>
        [HttpGet]
        [Route("attachmentTypes")]
        public async Task<string[]> GetAccessibleDocumentTypesByRoleIdAsync(long roleId, string entityType, long entityId)
        {
            var result = await _attachmentService.GetAccessibleDocumentTypesAsync(roleId, entityType, entityId, CurrentUser.OrganizationId);
            return result;
        }

        private async Task<ShareDocumentViewModel> CreateCompressedShareFile(IEnumerable<AttachmentModel> listDocument)
        {
            ShareDocumentViewModel shareDocument = new ShareDocumentViewModel();

            using (MemoryStream memory = new MemoryStream())
            {
                using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
                {
                    foreach (var document in listDocument)
                    {
                        var entryFile = archive.CreateEntry(document.FileName, CompressionLevel.NoCompression);
                        var docFile = await _blobStorage.GetBlobAsByteArrayAsync(document.BlobId);

                        using (var entryContent = entryFile.Open())
                        {
                            using (var binWriter = new BinaryWriter(entryContent))
                            {
                                binWriter.Write(docFile);
                            }
                        }
                    }
                }

                memory.Flush();
                memory.Position = 0;

                string shareFileName = $"SP_Attachment_Shared_{DateTime.Now.ToString("MMddyyyyHHmmss")}.zip";
                shareDocument.BlobId = await _blobStorage.PutBlobAsync(BlobCategories.SHARE, shareFileName, memory);
                shareDocument.FileName = shareFileName;
                shareDocument.SharedBy = CurrentUser.Username;
                shareDocument.Audit(CurrentUser.Name);

                shareDocument = await _shareDocumentService.CreateAsync(shareDocument);
            }

            return shareDocument;
        }
    }
}
